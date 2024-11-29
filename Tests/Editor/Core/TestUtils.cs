using System.Collections;
using System.Threading.Tasks;

namespace FAMOZ.ExternalProgram.Tests.Editor.Core
{
    public static class TestUtils
    {
        public static IEnumerator AsCoroutine(this Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted && task.Exception != null)
            {
                throw task.Exception.GetBaseException();
            }
        }

        public static IEnumerator AsCoroutine<T>(this Task<T> task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted && task.Exception != null)
            {
                throw task.Exception.GetBaseException();
            }
        }
    }
}