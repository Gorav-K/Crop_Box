using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// LimitedQueue code found on StackOverflow
/// https://stackoverflow.com/questions/1292/limit-size-of-queuet-in-net
/// </summary>

namespace CropBox.Models
{
    public class LimitedQueue<T> : Queue<T>
    {
        public int Limit { get; set; }

        public LimitedQueue(int limit) : base(limit)
        {
            Limit = limit;
        }

        public new void Enqueue(T item)
        {
            while (Count >= Limit)
            {
                Dequeue();
            }
            base.Enqueue(item);
        }
    }
}
