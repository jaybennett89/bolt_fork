namespace UdpKit {
    class UdpRingBuffer<T> where T : struct {
        int head;
        int tail;
        int count;

        readonly T[] array;

        public bool Full {
            get { return count == array.Length; }
        }

        public bool Empty {
            get { return count == 0; }
        }

        public float FillRatio {
            get { return UdpMath.Clamp((float) count / (float) array.Length, 0f, 1f); }
        }

        public UdpRingBuffer (int size) {
            array = new T[size];
        }

        public void Enqueue (T item) {
            if (count == array.Length)
                throw new UdpException("buffer is full");

            array[head] = item;
            head = (head + 1) % array.Length;
            count += 1;
        }

        public T Dequeue () {
            if (count == 0)
                throw new UdpException("buffer is empty");

            T item = array[tail];
            array[tail] = default(T);
            tail = (tail + 1) % array.Length;
            count -= 1;
            return item;
        }

        public T Peek () {
            if (count == 0)
                throw new UdpException("buffer is empty");

            return array[tail];
        }
    }
}
