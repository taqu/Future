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

    internal class SharedState<T>
    {
        public SharedState()
        {
            value_ = default(T);
        }

        public SharedState(T value)
        {
            value_ = value;
        }

        public bool IsReady
        {
            get { lock(event_) { return isReady_; } }
        }

        public void close()
        {
            event_.Close();
        }

        public void set(T value)
        {
            lock(event_) {
                isReady_ = true;
                value_ = value;
            }
            event_.Set();
        }

        public T get()
        {
            if(!IsReady) {
                throw new ExceptionNoState();
            }
            lock(event_) {
                return value_;
            }
        }

        public void wait()
        {
            event_.WaitOne();
        }

        public bool waitFor(int milliseconds)
        {
            return (0<milliseconds)? event_.WaitOne(milliseconds) : IsReady;
        }

        private ManualResetEvent event_ = new ManualResetEvent(false);
        private bool isReady_;
        private T value_;
    };

    public struct Future<T>
    {
        public enum Status
        {
            Ready,
            Timeout,
        }

        internal Future(SharedState<T> sharedState)
        {
            sharedState_ = sharedState;
        }

        public bool IsReady
        {
            get {return sharedState_.IsReady; }
        }

        public T get()
        {
            if(null == sharedState_) {
                throw new ExceptionBrokenPromise();
            }
            T result = sharedState_.get();
            sharedState_ = null;
            return result;
        }

        public void wait()
        {
            if(null == sharedState_) {
                throw new ExceptionBrokenPromise();
            }
            sharedState_.wait();
        }

        public Status waitFor(int milliseconds)
        {
            return sharedState_.waitFor(milliseconds) ? Status.Ready : Status.Timeout;
        }

        private SharedState<T> sharedState_;
    };

    public class Promise<T>
    {
        public Promise()
        {
            sharedState_ = new SharedState<T>();
        }

        public Promise(T value)
        {
            sharedState_ = new SharedState<T>(value);
        }

        public Future<T> getFuture()
        {
            return new Future<T>(sharedState_);
        }

        public void set(T value)
        {
            sharedState_.set(value);
        }

        public void close()
        {
            sharedState_.close();
        }

        private SharedState<T> sharedState_;
    };
}
