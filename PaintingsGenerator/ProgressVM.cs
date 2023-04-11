using System;

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
                curProgress = Math.Min(value, MaxProgress);
                status = $"{curProgress}/{maxProgress}";

                NotifyPropertyChanged(nameof(Status));
                NotifyPropertyChanged(nameof(CurProgress));
            }
        }
    }
}
