namespace UoT.hacks {
  public class DefaultAnimationBankHack {
    public static string? GetDefaultAnimationBankForObject(string filename) {
      if (filename == "object_link_boy" ||
          filename == "object_link_child" ||
          filename == "object_torch2") {
        return "link_animetion";
      }

      return null;
    }
  }
}