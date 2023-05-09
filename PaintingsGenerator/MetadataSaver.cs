using System.IO;
using System.Text.Json;

using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator {
    public class MetadataSaver {
        private StreamWriter? metadata;
        private bool haveARecord = false;

        public void StartWrite(string pathToMetadata) {
            if (metadata != null) metadata.Close();

            haveARecord = false;
            metadata = new(pathToMetadata + "_metadata.json", false);
            metadata.Write("{\"parameters\": [");
        }

        public void Write(StrokeParameters parameters) {
            if (metadata == null) return;
            
            if (haveARecord) metadata.Write(',');
            var paramInJson = JsonSerializer.Serialize(parameters);
            metadata.Write(paramInJson);
            
            haveARecord = true;
        }

        public void EndWrite() {
            if (metadata != null) {
                metadata.Write("]}");
                metadata.Close();
            }
        }
    }
}
