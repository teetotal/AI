using System;
using System.Collections.Generic;

namespace ENGINE {
    public class ObjectPool<T> where T : new() {
        protected Stack<T> mPool = new Stack<T>();
        protected int mCntNew = 0;
        protected int mCntPop = 0;
        protected int mCntAlloc = 0;
        protected int mCntRelease = 0;
        public T Alloc() {
            
            mCntAlloc++;

            if(mPool.Count > 0) {
                mCntPop++;
                return mPool.Pop();
            }
            
            T p = new T();
            mCntNew++;
            
            return p;
        }
        public void Release(T p) {
            mCntRelease++;
            mPool.Push(p);
        }
        public int GetCntRelease() {
            return mCntRelease;
        }
        public int GetCntAlloc() {
            return mCntAlloc;
        }
        public int GetCntPop() {
            return mCntPop;
        }
        public int GetCntNew() {
            return mCntNew;
        }
        public string GetDebugString() {
            return string.Format("Alloc(new, pop): [{0}, {1}] / {2}, Release: {3}", GetCntNew(), GetCntPop(), GetCntAlloc(), GetCntRelease());
        }
    }
}