using Newtonsoft.Json;

namespace stream_aggregator_3.Models
{
  public class EventStream
  {
    [JsonProperty("eventId")]
    public int EventId { get; set; }
    [JsonProperty("streamId")]
    public int StreamId { get; set; }
    [JsonProperty("event")]
    public string Event { get; set; }
    public Data data { get; set; }
  }

  public class Data
  {
    public string Name { get; set; }
    public int? PersonId { get; set; }
  }
}