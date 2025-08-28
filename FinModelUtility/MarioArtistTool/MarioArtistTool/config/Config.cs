using fin.io;

namespace MarioArtistTool.config;

public class Config {
  private const string FILE_NAME = "./config.json";

  public static Config INSTANCE { get; } = LoadConfig_();

  private static Config LoadConfig_() {
    var file = new FinFile(FILE_NAME);
    if (file.Exists) {
      return file.Deserialize<Config>();
    }

    return new Config();
  }

  public void Save() => new FinFile(FILE_NAME).Serialize(this);

  public string? MostRecentDiskFile { get; set; }
  public string? MostRecentFileName { get; set; }
}