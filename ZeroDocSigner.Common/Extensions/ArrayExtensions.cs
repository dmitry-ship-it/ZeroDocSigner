using System.Numerics;
using System.Text;

namespace ZeroDocSigner.Common.Extensions
{
    public static class ArrayExtensions
    {
        public static long FindLastSequenceIndex<T>(this T[] array, T[] sequence)
            where T : IEqualityOperators<T, T, bool>
        {
            if (sequence.LongLength > array.LongLength)
            {
                return -1;
            }

            for (var i = array.LongLength - 1; i >= 0L; i--)
            {
                if (array[i] == sequence[^1])
                {
                    var targetIndex = i;
                    var j = sequence.LongLength - 1;

                    while (targetIndex != -1L
                        && j != -1L
                        && array[targetIndex] == sequence[j])
                    {
                        targetIndex--;
                        j--;
                    }

                    if (j == -1)
                    {
                        return i - sequence.LongLength + 1;
                    }

                    i = targetIndex;
                }
            }

            return -1;
        }

        public static T[] StickWith<T>(this T[] first, T[] second)
        {
            if (second.Length == 0)
            {
                return first;
            }

            if (second.Length == 1)
            {
                return first.Add(second[0]);
            }

            var result = new T[first.Length + second.Length];

            Array.Copy(first, result, first.Length);
            Array.Copy(second, 0,
                result, first.Length, second.Length);

            return result;
        }

        public static T[] RemoveOne<T>(this T[] array, Predicate<T> expression)
        {
            var index = Array.FindIndex(array, expression);

            if (index == -1)
            {
                return array;
            }

            var result = new T[array.Length - 1];

            Array.Copy(array, result, index);
            Array.Copy(array, index + 1,
                result, index, array.Length - index);

            return result;
        }

        public static T[] Take<T>(this T[] array, long count)
        {
            var result = new T[count];

            Array.Copy(array, result, count);

            return result;
        }

        public static T[] TakeFrom<T>(this T[] array, long index)
        {
            if (array.Length <= index)
            {
                return array;
            }

            var result = new T[array.Length - index];
            Array.Copy(array, index, result, 0L, result.Length);

            return result;
        }

        public static T[] Add<T>(this T[] array, T item)
        {
            var result = new T[array.Length + 1];

            Array.Copy(array, result, array.Length);
            result[^1] = item;

            return result;
        }
    }
}
