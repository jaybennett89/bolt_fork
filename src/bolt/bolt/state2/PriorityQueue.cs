/*
 * SlimNet - Networking Middleware For Games
 * Copyright (C) 2011-2012 Fredrik Holmström
 * 
 * This notice may not be removed or altered.
 * 
 * This software is provided 'as-is', without any expressed or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software. 
 * 
 * Attribution
 * The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. For any works using this 
 * software, reasonable acknowledgment is required.
 * 
 * Noncommercial
 * You may not use this software for commercial purposes.
 * 
 * Distribution
 * You are not allowed to distribute or make publicly available the software 
 * itself or its source code in original or modified form.
 */

using System;
using System.Collections.Generic;

namespace Bolt {
  public class PriorityQueue<V> {
    public struct Entry {
      public float Key;
      public V Value;
    }

    int count;
    Entry[] heap;

    public Entry[] Entries {
      get { return heap; }
    }

    public int Count {
      get { return count; }
    }

    public PriorityQueue(int capacity) {
      heap = new Entry[Math.Max(capacity, 2)];
      count = 0;
    }

    public Entry Peek() {
      ensureNotEmpty();
      return heap[0];
    }

    public void Add(float key, V value) {
      if (count >= heap.Length) {
        throw new IndexOutOfRangeException();
      }

      heap[count].Key = key;
      heap[count].Value = value;

      Entry tmp;
      int index = count;
      int parent = 0;

      while (index != 0) {
        parent = (index - 1) / 2;

        if (heap[index].Key.CompareTo(heap[parent].Key) == -1) {
          tmp = heap[parent];
          heap[parent] = heap[index];
          heap[index] = tmp;
          index = parent;
        }
        else {
          break;
        }
      }

      ++count;
    }

    public V Remove() {
      ensureNotEmpty();

      V result = heap[0].Value;
      count -= 1;

      if (count > 0) {
        heap[0] = heap[count];

        // Must clear the last value so we don't leak memory
        heap[count] = default(Entry);

        // Lets trickle the item downwards
        Entry tmp;
        int swap = 0;
        int index = 0;

        do {
          index = swap;

          int left = (index * 2) + 1;
          int right = (index * 2) + 2;

          if (right < count) {
            if (heap[index].Key.CompareTo(heap[left].Key) != -1) {
              swap = left;
            }

            if (heap[swap].Key.CompareTo(heap[right].Key) != -1) {
              swap = right;
            }
          }
          else if (left < count) {
            if (heap[index].Key.CompareTo(heap[left].Key) != -1) {
              swap = left;
            }
          }

          if (index != swap) {
            tmp = heap[index];
            heap[index] = heap[swap];
            heap[swap] = tmp;
          }

        } while (index != swap);
      }
      else {
        heap[0] = default(Entry);
      }

      return result;
    }

    void ensureNotEmpty() {
      if (count < 1) {
        throw new InvalidOperationException("The heap is empty");
      }
    }

    public IEnumerator<V> GetEnumerator() {
      for (int i = 0; i < count; ++i) {
        yield return heap[i].Value;
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }
}