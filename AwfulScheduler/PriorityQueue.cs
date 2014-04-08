﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Awful.Scheduler
{
    class PriorityQueue<TPriority, TValue> //: ICollection , IEnumerable<PriorityQueue<TPriority, TValue>>
    {
        private SortedDictionary<TPriority, Queue<TValue>> peripheral = new SortedDictionary<TPriority, Queue<TValue>>();
        public void enqueue(TPriority priority, TValue value)
        {
            Queue<TValue> central = null;
            if (!peripheral.TryGetValue(priority, out central))
            {
                central = new Queue<TValue>();
                peripheral.Add(priority, central);
            }
            central.Enqueue(value);
        }

        public TValue dequeue()
        {
            // will throw if there isn’t any first element!
            var central = peripheral.First();
            var value = central.Value.Dequeue();
            // nothing left of the top priority.
            if (central.Value.Count == 0) 
                peripheral.Remove(central.Key);
            return value;
        }

        public TValue peek()
        {
            // will throw if there isn’t any first element!
            var central = peripheral.First();
            var value = central.Value.Peek();
            return value;
        }

        public bool empty()
        {
            return !peripheral.Any();
        }

        public bool containsPair(TPriority priority, TValue value)
        {
            if (empty())
            {
                return false;
            }
            Queue<TValue> central = null;
            if (peripheral.TryGetValue(priority, out central) == true)
            {
                return central.Contains(value);
            }
            return false;
        }
    }
    
}
