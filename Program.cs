using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Future
{
    class Program
    {
        private static void proc(object arg)
        {
            Promise<int> promise = arg as Promise<int>;
            Thread.Sleep(1000);
            promise.set(1);
        }

        static void Main(string[] args)
        {
            Promise<int> promise = new Promise<int>();
            Future<int> future = promise.getFuture();
            Thread thread = new Thread(proc);
            thread.Start(promise);
            future.wait();
            Console.WriteLine(string.Format("{0}", future.get()));
            future = promise.getFuture();
            future.wait();
            Console.WriteLine(string.Format("{0}", future.get()));
            thread.Join();
        }
    }
}
