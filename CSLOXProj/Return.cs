namespace CSLOXProj {
    class Return : LoxExceptions {
        public object Value { get; }

        public Return(object value) : base() {
            this.Value = value;
        }
    }
}
