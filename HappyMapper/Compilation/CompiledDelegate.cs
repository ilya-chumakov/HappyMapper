namespace HappyMapper.Compilation
{
    internal class CompiledDelegate
    {
        /// <summary>
        /// Func&lt;TSrc, TDest, TDest&gt; is expected.
        /// </summary>
        public object SingleTyped { get; set; }
        /// <summary>
        /// Func&lt;object, TDest&gt; is expected.
        /// </summary>
        public object SingleUntyped { get; set; }
        /// <summary>
        /// Func&lt;ICollection&lt;TSrc&gt;, ICollection&lt;TDest&gt;, ICollection&lt;TDest&gt;&gt; is expected.
        /// </summary>
        public object CollectionTyped { get; set; }
        /// <summary>
        /// Action&lt;object, object&gt; is expected.
        /// </summary>
        public object CollectionUntyped { get; set; }
    }
}