namespace FieldDay.Collections {
    /// <summary>
    /// Double-buffered object set.
    /// </summary>
    public struct DoubleBuffered<T> {
        public T Current;
        public T Back;

        public T Next() {
            T temp = Back;
            Back = Current;
            Current = temp;
            return temp;
        }
    }

    /// <summary>
    /// Triple-buffered object set.
    /// </summary>
    public struct TripleBuffered<T> {
        public T Current;
        public T Back0;
        public T Back1;

        public T Next() {
            T temp = Back1;
            Back1 = Back0;
            Back0 = Current;
            Current = temp;
            return temp;
        }
    }
}