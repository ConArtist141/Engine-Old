using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Engine
{
    public class ArrayCache<Type> : IEnumerable<Type>
    {
        protected Type[] array;
        protected int count = 0;

        public Type this[int i]
        {
            get { return array[i]; }
            set { array[i] = value; }
        }

        public int Count
        {
            get { return count; }
        }

        public int Size
        {
            get { return array.Length; }
        }

        public ArrayCache(int initialSize)
        {
            array = new Type[initialSize];
        }

        public void Clear()
        {
            count = 0;
        }

        public void Resize(int newSize)
        {
            var oldArray = array;
            array = new Type[newSize];

            if (newSize >= oldArray.Length)
                oldArray.CopyTo(array, 0);
        }

        public void Add(Type obj)
        {
            if (count == array.Length)
                Resize(array.Length * 2);

            array[count] = obj;
            ++count;
        }

        public IEnumerator<Type> GetEnumerator()
        {
            return array.Take(count).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return array.GetEnumerator();
        }
    }
}
