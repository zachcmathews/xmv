using System;
using System.Collections.Generic;

namespace Xmv
{
  internal class Scheduler
  {
    static public Queue<Task> Queue { get; set; } = new Queue<Task> ();

    static public void DoNext()
    {
      if (Queue.Count == 0) return;
      Queue.Dequeue().Action.Invoke();
    }
  }

  internal class Task
  {
    public Guid Source { get; set; }
    public Action Action { get; set; }
  }
}
