using System.IO;
using System.Text;

using fin.util.asserts;

namespace fin.io;

public class CsvWriter {
  private readonly StringBuilder impl_ = new();
  private readonly char separator_;

  private readonly int cellsPerRow_;
  private int currentCellInRow_ = 0;

  public CsvWriter(char separator, string firstHeader, params string[] otherHeaders) {
    this.separator_ = separator;
    this.cellsPerRow_ = 1 + otherHeaders.Length;

    this.Append(firstHeader);
    foreach (var header in otherHeaders) {
      this.Append(header);
    }
    this.EndRow();
  }

  public CsvWriter Append(string value) {
    Asserts.True(this.currentCellInRow_++ < this.cellsPerRow_,
                 "Attempted to write an extra cell in a row!");
    Asserts.False(value.Contains(this.separator_),
                  $"Expected CSV value \"{value}\" not to contain the separator \"{this.separator_}\"");

    this.impl_.Append(value);
    this.impl_.Append(this.separator_);

    return this;
  }

  public CsvWriter EndRow() {
    Asserts.True(this.currentCellInRow_ == this.cellsPerRow_,
                 $"Ending line with only {this.currentCellInRow_}/{this.cellsPerRow_} elements!");
    this.impl_.Append('\n');
    this.currentCellInRow_ = 0;
    return this;
  }

  public void WriteToFile(ISystemFile outputFile)
    => File.WriteAllText(outputFile.FullPath, this.impl_.ToString());
}