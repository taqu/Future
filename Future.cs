using System;
using System.Threading;

namespace Future
{
    public class ExceptionNoState : Exception
    {
    };

    public class ExceptionBrokenPromise : Exception
    {
    };

    public struct Future<T>
    {
        public enum Status
        {
            Ready,
            Timeout,
        }

        public Future(Promise<T> promise)
        {
            promise_ = promise;
        }

        public T get()
        {
            if(null == promise_) {
                throw new ExceptionBrokenPromise();
            }
            T result = promise_.get();
            promise_ = null;
            return result;
        }

        public void wait()
        {
            if(null == promise_) {
                throw new ExceptionBrokenPromise();
            }
            promise_.wait();
        }

        public Status waitFor(int milliseconds)
        {
            return promise_.waitFor(milliseconds) ? Status.Ready : Status.Timeout;
        }

        private Promise<T> promise_;
    };

    public class Promise<T>
    {
        public Promise()
        {
            value_ = default(T);
        }

        public Promise(T value)
        {
            value_ = value;
        }

        public bool IsReady
        {
            get { lock(object_) { return isReady_; } }
        }

        public Future<T> getFuture()
        {
            return new Future<T>(this);
        }

        public void set(T value)
        {
            lock(object_) {
                isReady_ = true;
                value_ = value;
            }
            object_.Set();
        }

        public T get()
        {
            if(!IsReady) {
                throw new ExceptionNoState();
            }
            lock(object_) {
                return value_;
            }
        }

        public void wait()
        {
            object_.WaitOne();
        }

        public bool waitFor(int milliseconds)
        {
            return (0<milliseconds)? object_.WaitOne(milliseconds) : IsReady;
        }

        private ManualResetEvent object_ = new ManualResetEvent(false);
        private bool isReady_;
        private T value_;
    };
}
