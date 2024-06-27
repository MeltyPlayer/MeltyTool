namespace fin.audio.midi;

public interface IInstrument {
  // TODO: How to handle when this changes dynamically?
  IEnvelope Envelope { get; }
}