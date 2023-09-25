using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<KeyValuePair<T, float>> elements = new List<KeyValuePair<T, float>>();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add(new KeyValuePair<T, float>(item, priority));
        HeapifyUp(elements.Count - 1);
    }

    public T Dequeue()
    {
        if (Count == 0)
        {
            return default(T);
        }
        
        T item = elements[0].Key;
        elements[0] = elements[elements.Count - 1];
        elements.RemoveAt(elements.Count - 1);

        HeapifyDown(0);
        return item;
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (elements[parentIndex].Value <= elements[index].Value)
            {
                break;
            }

            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    private void HeapifyDown(int index)
    {
        while (true)
        {
            int childIndex = index * 2 + 1;
            if (childIndex >= elements.Count)
            {
                break;
            }

            if (childIndex + 1 < elements.Count && elements[childIndex].Value > elements[childIndex + 1].Value)
            {
                childIndex++;
            }

            if (elements[childIndex].Value >= elements[index].Value)
            {
                break;
            }

            Swap(index, childIndex);
            index = childIndex;
        }
    }

    private void Swap(int indexA, int indexB)
    {
        KeyValuePair<T, float> temp = elements[indexA];
        elements[indexA] = elements[indexB];
        elements[indexB] = temp;
    }
}