namespace PaintingsGenerator {
    internal class ProgressVM : NotifierOfPropertyChange {
        private string status = "";
        private uint maxProgress = 100;
        private uint curProgress = 0;

        public string Status => status;
        public uint MaxProgress {
            get => maxProgress;
            set {
                maxProgress = value;
                NotifyPropertyChanged(nameof(MaxProgress));
            }
        }
        public uint CurProgress {
            get => curProgress;
            set {
                status = $"{curProgress}/{maxProgress}";
                curProgress = value;

                NotifyPropertyChanged(nameof(Status));
                NotifyPropertyChanged(nameof(CurProgress));
            }
        }
    }
}
