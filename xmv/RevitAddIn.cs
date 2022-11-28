using System.Collections.Generic;
using System.Text.Json;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;

namespace Xmv
{
  using Autodesk.Revit.DB.ExtensibleStorage;
  using Autodesk.Revit.UI.Events;
  using System;
  using System.Linq;
  using Xmv.Models;
  using Xmv.ViewModels;
  using Xmv.Views;

  internal class Resources
  {
    public static Dictionary<string, Validator> Validators { get; set; } = new Dictionary<string, Validator>();
  }

  internal class Scheduler
  {
    public Queue<Action> Queue { get; set; } = new Queue<Action> ();
  }

  internal class SerializableConfiguration
  {
    public List<string> TestDirectories { get; set; } = new List<string>();
    public List<string> TestFiles { get; set; } = new List<string>();
  }

  [Transaction(TransactionMode.Manual)]
  [Regeneration(RegenerationOption.Manual)]
  public class RevitAddIn : IExternalApplication
  {
    UIControlledApplication uiControlledApplication;
    UIApplication uiapp;
    Guid schemaGuid = new Guid("1f7b0fe4-bd91-41e2-b887-f445f0aec557");
    string schemaName = "eXtensibleModelValidatorConfiguration";
    Scheduler scheduler = new Scheduler();

    public Result OnStartup(UIControlledApplication application)
    {
      uiControlledApplication = application;

      application.Idling += Initialize;
      application.Idling += Idling;
      application.ControlledApplication.DocumentOpened += ControlledApplication_DocumentOpened;
      application.ControlledApplication.DocumentClosing += ControlledApplication_DocumentClosing;
      return Result.Succeeded;
    }

    // We have to wait to get the UI Application from the idling event
    private void Initialize(object sender, IdlingEventArgs e)
    {
      uiControlledApplication.Idling -= Initialize;
      uiapp = sender as UIApplication;
    }

    private void Idling(object sender, IdlingEventArgs e)
    {
      if (scheduler.Queue.Count == 0) return;
      scheduler.Queue.Dequeue().Invoke();
    }

    // Idling should run at some point before this event
    private void ControlledApplication_DocumentOpened(object sender, DocumentOpenedEventArgs e)
    {
      var document = e.Document;

      var (_, configurationEntity) = GetConfigurationEntity(document, schemaGuid);

      // Parse the stored configuration
      SerializableConfiguration serializableConfiguration;
      if (configurationEntity != null)
      {
        try
        {
          var configurationStr = configurationEntity.Get<string>(schemaName);
          serializableConfiguration = JsonSerializer.Deserialize<SerializableConfiguration>(configurationStr);
        }
        catch
        {
          // TODO: Alert user
          return;
        }
      }
      // Otherwise, create a new configuration and store it
      else
      {
        using (var t = new Transaction(document))
        {
          t.Start("Create new eXtensible Model Validator Configuration");

          // Create the schema
          var schemaBuilder = new SchemaBuilder(schemaGuid);
          schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
          schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
          schemaBuilder.SetSchemaName(schemaName);
          schemaBuilder.AddSimpleField(schemaName, typeof(string));
          var schema = schemaBuilder.Finish();

          // Construct a new serializable configuration
          // that we'll store in our newly created entity
          serializableConfiguration = new SerializableConfiguration { };

          // Create a new entity with our configuration schema and store it
          var storage = DataStorage.Create(document);
          var entity = new Entity(schema);
          entity.Set(schemaName, JsonSerializer.Serialize(serializableConfiguration));
          storage.SetEntity(entity);

          t.Commit();
        }
      }

      // Add runtime information to our configuration.
      // This will be passed to instantiated tests.
      var configuration = new Configuration
      {
        Name =
            (document.IsWorkshared && !document.IsDetached) ?
              ModelPathUtils.ConvertModelPathToUserVisiblePath(
                document.GetWorksharingCentralModelPath())
              : document.PathName,
        Context = new object[] { uiapp, document },
        TestDirectories = serializableConfiguration.TestDirectories,
        TestFiles = serializableConfiguration.TestFiles,
      };

      var validator = new Validator(configuration);
      validator.ConfigurationChanged += Validator_ConfigurationChanged;
      Resources.Validators.Add(document.PathName, validator);
    }

    private (DataStorage, Entity) GetConfigurationEntity(Document document, Guid schemaGuid)
    {
      var dataStorage =
        new FilteredElementCollector(document)
        .OfClass(typeof(DataStorage))
        .OfType<DataStorage>()
        .Where(ds => ds.GetEntitySchemaGuids().Contains(schemaGuid))
        .FirstOrDefault();

      if (dataStorage == null) return (null, null);

      var schema = Schema.Lookup(schemaGuid);
      var entity = dataStorage.GetEntity(schema);
      return (dataStorage, entity);
    }

    private void Validator_ConfigurationChanged(object sender, EventArgs e)
    {
      var validator = sender as Validator;
      var document = validator.Configuration.Context[1] as Document;

      // TODO: Might need to add some notification here.
      // But entity should only be null in an exceptional circumstance.
      var (dataStorage, entity) = GetConfigurationEntity(document, schemaGuid);
      if (entity == null) { return; }

      var serializableConfiguration = new SerializableConfiguration
      {
        TestDirectories = validator.Configuration.TestDirectories,
        TestFiles = validator.Configuration.TestFiles,
      };

      // We have to update the configuration within a Revit event handler
      // then immediately remove our handler.
      Action handler = () =>
      {
        using (var t = new Transaction(document))
        {
          if (t.Start("Update eXtensible Model Validator configuration") != TransactionStatus.Started) return;
          var serializedConfig = JsonSerializer.Serialize(serializableConfiguration);
          entity.Set(schemaName, serializedConfig); 
          dataStorage.SetEntity(entity);
          if (t.Commit() != TransactionStatus.Committed) return;
        }
      };
      scheduler.Queue.Enqueue(handler);
    }

    private void ControlledApplication_DocumentClosing(object sender, DocumentClosingEventArgs e)
    {
      if (Resources.Validators.ContainsKey(e.Document.PathName))
      {
        Resources.Validators.Remove(e.Document.PathName);
      }
    }

    public Result OnShutdown(UIControlledApplication application)
    {
      return Result.Succeeded;
    }
  }

  [Transaction(TransactionMode.Manual)]
  [Regeneration(RegenerationOption.Manual)]
  public class Monitor : IExternalCommand
  {
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
      var uidoc = commandData.Application.ActiveUIDocument;
      var document = uidoc.Document.PathName;

      Validator validator;
      Resources.Validators.TryGetValue(document, out validator);
      if (validator == null)
      {
        // TODO: Alert user
        return Result.Failed;
      }

      var validatorVM = new ValidatorVM(validator);
      var validatorView = new ValidatorView(validatorVM);

      validatorView.Show();
      return Result.Succeeded;
    }
  }
}
