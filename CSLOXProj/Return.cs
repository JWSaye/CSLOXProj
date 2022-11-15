namespace CSLOXProj {
    class Return : LoxExceptions {
        public readonly object value;

        public Return(object value) : base() {
            this.value = value;
        }
    }
}
