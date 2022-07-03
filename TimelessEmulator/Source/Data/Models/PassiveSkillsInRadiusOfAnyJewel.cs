using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TimelessEmulator.Data.Models;

public class PassiveSkillsInRadiusOfAnyJewel
{

    [JsonPropertyName("identifier")]
    public string Identifier { get; init; }

    [JsonPropertyName("Name")]
    public string Name { get; init; }

    public PassiveSkillsInRadiusOfAnyJewel()
    {
        this.Identifier = default;
        this.Name = default;
    }

}
