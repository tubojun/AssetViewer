﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RDA.Data {

  public class Upgrade {

    #region Properties

    public Description Text { get; set; }
    public String Value { get; set; }
    public List<AdditionalOutput> AdditionalOutputs { get; set; }
    public List<ReplaceInput> ReplaceInputs { get; set; }
    public List<InputAmountUpgrade> InputAmountUpgrades { get; set; }
    public ReplacingWorkforce ReplacingWorkforce { get; set; }
    public List<Upgrade> Additionals { get; set; }

    #endregion Properties

    #region Constructors

    public Upgrade() {
    }

    public Upgrade(XElement element) {
      var isPercent = element.Element("Percental") == null ? false : element.Element("Percental").Value == "1";
      var value = element.Element("Value") == null ? null : (Int32?)Int32.Parse(element.Element("Value").Value);
      var factor = 1;
      if (Assets.Descriptions.ContainsKey(element.Name.LocalName)) {
        this.Text = new Description(Assets.Descriptions[element.Name.LocalName]);
      }
      switch (element.Name.LocalName) {
        case "PassiveTradeGoodGenUpgrade":
          this.Text.AdditionalInformation = new Description("20327", DescriptionFontStyle.Light);
          var genpool = element.Element("GenPool").Value;
          var items = Assets
            .Original
            .Descendants("Asset")
            .FirstOrDefault(a => a.XPathSelectElement("Values/Standard/GUID")?.Value == genpool)
            .XPathSelectElement("Values/RewardPool")
            .Elements("Item")
            .Select(i => i.Element("ItemLink").Value)
            .Select(i => new Description(i));
          this.Text.AdditionalInformation.EN.Replace("[ItemAssetData([RefGuid]) GoodGenerationPoolFormatted]", string.Join(",", items.Select(d => d.EN)));
          this.Text.AdditionalInformation.DE.Replace("[ItemAssetData([RefGuid]) GoodGenerationPoolFormatted]", string.Join(",", items.Select(d => d.DE)));
          value = Convert.ToInt32(element.Value);
          isPercent = true;
          break;

        case "AddAssemblyOptions":
          this.Text.AdditionalInformation = new Description("20325", DescriptionFontStyle.Light);
          var descs = element.Elements("Items").Select(i => new Description(i.Element("NewOption").Value));
          this.Text.AdditionalInformation.EN.Replace("[ItemAssetData([RefGuid]) AddAssemblyOptionsFormatted]", string.Join(",", descs.Select(d => d.EN)));
          this.Text.AdditionalInformation.DE.Replace("[ItemAssetData([RefGuid]) AddAssemblyOptionsFormatted]", string.Join(",", descs.Select(d => d.DE)));
          break;

        case "MoraleDamage":
          this.Text.AdditionalInformation = new Description("21586", DescriptionFontStyle.Light);
          break;

        case "HitpointDamage":
          switch (element.Parent.Parent.Element("Item").Element("Allocation").Value) {
            case "Ship":
            case "SailShip":
            case "Warship":
            case "SteamShip":
              this.Text.AdditionalInformation = new Description("21585", DescriptionFontStyle.Light);
              break;

            default:
              this.Text.AdditionalInformation = new Description("21589", DescriptionFontStyle.Light);
              break;
          }
          break;

        case "SpecialUnitHappinessThresholdUpgrade":
          this.Text.AdditionalInformation = new Description("21584", DescriptionFontStyle.Light);
          var target = element.Parent.Parent.Element("ItemEffect").Element("EffectTargets").Elements().FirstOrDefault().Element("GUID").Value;
          Description unit = null;
          switch (target) {
            case "190777": //Hospital
              unit = new Description("100584");
              //volunteer = new Description("100583");
              break;

            case "190776": //Police Station
              unit = new Description("100582");
              //volunteer = new Description("100581");
              break;

            case "190775": //Fire Station
              unit = new Description("100580");
              //volunteer = new Description("100579");
              break;

            default:
              throw new NotImplementedException(target);
          }
          this.Text.AdditionalInformation.EN = this.Text.EN.Replace("[AssetData([ToolOneHelper IncidentResolverUnitsForTargetBuildings([RefGuid], 1) AT(0)]) Text]", unit.EN);
          this.Text.AdditionalInformation.DE = this.Text.DE.Replace("[AssetData([ToolOneHelper IncidentResolverUnitsForTargetBuildings([RefGuid], 1) AT(0)]) Text]", unit.DE);
          break;

        case "ItemSet":
        case "ProvidedNeed":
          this.Text = new Description(element.Value);
          break;

        case "HappinessIgnoresMorale":
          this.Text.AdditionalInformation = new Description("20326", DescriptionFontStyle.Light);
          break;

        case "AdditionalSupply":
          this.Text = new Description("12687");
          this.Text.DE = this.Text.DE.Replace(" [AssetData([ItemAssetData([RefGuid]) InputBenefitModifierProduct(index)]) Text]", "");
          this.Text.EN = this.Text.DE.Replace(" [AssetData([ItemAssetData([RefGuid]) InputBenefitModifierProduct(index)]) Text]", "");
          value = Int32.Parse(element.Value);
          break;

        case "ChangedSupplyValueUpgrade":
          this.Text = new Description("12649");
          this.Additionals = new List<Upgrade>();
          foreach (var item in element.Elements("Item")) {
            this.Additionals.Add(new Upgrade() { Text = new Description(item.Element("Need").Value), Value = (item.Element("AmountInPercent").Value.StartsWith("-") ? "" : "+") + $"{item.Element("AmountInPercent").Value}%" });
          }
          break;

        case "ResolverUnitDecreaseUpgrade":
          target = element.Parent.Parent.Element("ItemEffect").Element("EffectTargets").Elements().FirstOrDefault().Element("GUID").Value;
          unit = null;
          switch (target) {
            case "190777": //Hospital
              this.Text = new Description("12012");
              //unit = new Description("100583");
              break;

            case "190776": //Police Station
              this.Text = new Description("21509");
              //unit = new Description("100581");
              break;

            case "190775": //Fire Station
            case "1010463": //Fire Department
              this.Text = new Description("21508");
              //unit = new Description("100579");
              break;

            default:
              throw new NotImplementedException(target);
          }
          break;

        case "ResolverUnitCountUpgrade":
          target = element
            .Parent
            .Parent
            .Element("ItemEffect")
            .Element("EffectTargets")
            .Elements()
            .FirstOrDefault()
            .Element("GUID")
            .Value;
          unit = null;
          switch (target) {
            case "190777": //Hospital
              this.Text = new Description("100583");
              break;

            case "190776": //Police Station
              this.Text = new Description("100581");
              break;

            case "190775": //Fire Station
            case "1010463": //Fire Department
              this.Text = new Description("100579");
              break;

            default:
              throw new NotImplementedException(target);
          }
          break;

        case "AdditionalOutput":
          this.AdditionalOutputs = new List<AdditionalOutput>();
          foreach (var item in element.Elements()) {
            this.AdditionalOutputs.Add(new AdditionalOutput(item));
          }
          break;

        case "ReplaceInputs":
          this.ReplaceInputs = new List<ReplaceInput>();
          foreach (var item in element.Elements()) {
            this.ReplaceInputs.Add(new ReplaceInput(item));
          }
          break;

        case "InputAmountUpgrade":
          this.InputAmountUpgrades = new List<InputAmountUpgrade>();
          foreach (var item in element.Elements()) {
            this.InputAmountUpgrades.Add(new InputAmountUpgrade(item));
          }
          break;

        case "AddedFertility":
          this.Text = new Description(element.Value);
          this.Text.EN += " Provided";
          this.Text.DE += " bereitgestellt";
          break;

        case "ActiveTradePriceInPercent":
          if (value == null && !element.HasElements) {
            value = Int32.Parse(element.Value);
            if (value < 100) {
              value = -(100 - value);
            }
            else {
              value = (value - 100);
            }
          }
          isPercent = true;
          break;

        case "ActivateWhiteFlag":
          this.Text.Icon = new Icon("data/ui/2kimages/main/icons/icon_claim_island.png");
          this.Text.AdditionalInformation = new Description("19487", DescriptionFontStyle.Light);
          break;

        case "ActivatePirateFlag":
          this.Text.Icon = new Icon("data/ui/2kimages/main/icons/icon_threat_melee_tint.png");
          this.Text.AdditionalInformation = new Description("17393", DescriptionFontStyle.Light);
          break;

        case "AttackSpeedUpgrade":
          if (value == null) {
            value = element.Value == null ? null : (Int32?)Int32.Parse(element.Value);
          }
          isPercent = true;
          break;

        case "SelfHealPausedTimeIfAttackedUpgrade":
          this.Text.AdditionalInformation = new Description("21590", DescriptionFontStyle.Light);
          value = value == -100 ? null : value;
          break;

        case "NeedProvideNeedUpgrade":
          var SubstituteNeeds = element.Descendants("SubstituteNeed").Select(i => new Description(i.Value));
          var ProvidedNeeds = element.Descendants("SubstituteNeed").Select(i => new Description(i.Value));
          this.Text.AdditionalInformation = new Description("20323", DescriptionFontStyle.Light);
          this.Text.AdditionalInformation.EN.Replace("[ItemAssetData([RefGuid]) AllSubstituteNeedsFormatted]", string.Join(",", SubstituteNeeds.Select(d => d.EN)));
          this.Text.AdditionalInformation.EN.Replace("[ItemAssetData([RefGuid]) AllProvidedNeedsFormatted]", string.Join(",", ProvidedNeeds.Select(d => d.EN)));

          this.Text.AdditionalInformation.DE.Replace("[ItemAssetData([RefGuid]) AllSubstituteNeedsFormatted]", string.Join(",", SubstituteNeeds.Select(d => d.DE)));
          this.Text.AdditionalInformation.DE.Replace("[ItemAssetData([RefGuid]) AllProvidedNeedsFormatted]", string.Join(",", ProvidedNeeds.Select(d => d.DE)));
          break;

        case "GoodConsumptionUpgrade":
          this.Additionals = new List<Upgrade>();
          foreach (var item in element.Elements("Item")) {
            this.Additionals.Add(new Upgrade() { Text = new Description(item.Element("ProvidedNeed").Value), Value = (item.Element("AmountInPercent").Value.StartsWith("-") ? "" : "+") + $"{item.Element("AmountInPercent").Value}%" });
          }
          break;

        case "UseProjectile":
          var Projectile = Assets
            .Original
            .Root
            .Descendants("Asset")
            .FirstOrDefault(a => a.XPathSelectElement($"Values/Standard/GUID")?.Value == element.Value);

          var infodesc = Projectile.XPathSelectElement("Values/Standard/InfoDescription")?.Value;
          if (infodesc == null) {
            this.Text = new Description(element.Parent.Parent.XPathSelectElement($"Standard/GUID").Value);
            break;
          }
          var infodescAsset = Assets.Original.Root.Descendants("Asset").FirstOrDefault(a => a.XPathSelectElement($"Values/Standard/GUID")?.Value == infodesc);
          if (infodescAsset != null) {
            this.Text = new Description(infodescAsset.XPathSelectElement("Values/Standard/InfoDescription").Value);
            this.Text.AdditionalInformation = new Description(infodescAsset.XPathSelectElement("Values/Standard/GUID").Value, DescriptionFontStyle.Light);
          }
          break;

        case "ActionDuration":
          this.Text.FontStyle = DescriptionFontStyle.Light;
          this.Text.DE = "Dauer";
          this.Text.EN = "Duration";
          this.Value = TimeSpan.FromMilliseconds(Convert.ToInt64(element.Value)).ToString("hh':'mm':'ss");
          while (this.Value.StartsWith("00:00:")) {
            this.Value = this.Value.Remove(0, 3);
          }
          return;

        case "ActionCooldown":
          this.Text.FontStyle = DescriptionFontStyle.Light;
          this.Text.DE = "Aufladung";
          this.Text.EN = "Cooldown";
          this.Value = TimeSpan.FromMilliseconds(Convert.ToInt64(element.Value)).ToString("hh':'mm':'ss");
          while (this.Value.StartsWith("00:00:")) {
            this.Value = this.Value.Remove(0, 3);
          }
          return;

        case "IsDestroyedAfterCooldown":
          this.Text.FontStyle = DescriptionFontStyle.Light;
          this.Text.DE = "Wird nach Gebrauch zerstört";
          this.Text.EN = "Destroyed after use";
          break;

        case "Building":
          this.Text = new Description("17394");
          value = Convert.ToInt32((Decimal.Parse(element.Element("Factor").Value, System.Globalization.CultureInfo.InvariantCulture) * 100) - 100);
          isPercent = true;
          break;
        case "SailShip":
          this.Text = new Description("17395");
          value = Convert.ToInt32((Decimal.Parse(element.Element("Factor").Value, System.Globalization.CultureInfo.InvariantCulture) * 100) - 100);
          isPercent = true;
          break;
        case "SteamShip":
          this.Text = new Description("17396");
          value = Convert.ToInt32((Decimal.Parse(element.Element("Factor").Value, System.Globalization.CultureInfo.InvariantCulture) * 100) - 100);
          isPercent = true;
          break;

        case "ReplacingWorkforce":
          this.ReplacingWorkforce = new ReplacingWorkforce(element.Value);
          break;

        case "BaseDamageUpgrade":
          value = value ?? 0;
          break;

        case "AdditionalMoney":
          value = Int32.Parse(element.Value);
          break;

        case "IncidentIllnessIncreaseUpgrade":
        case "IncidentRiotIncreaseUpgrade":
        case "IncidentFireIncreaseUpgrade":
        case "IncidentExplosionIncreaseUpgrade":
          factor = 10;
          isPercent = true;
          break;

        case "Normal":
        case "Cannon":
        case "BigBertha":
        case "Torpedo":
          value = -Convert.ToInt32((100M - (100M * Decimal.Parse(element.Element("Factor").Value, CultureInfo.InvariantCulture))));
          isPercent = true;
          break;

        case "ConstructionTimeInPercent":
        case "ConstructionCostInPercent":
        case "TaxModifierInPercent":
        case "WorkforceModifierInPercent":
          value = Int32.Parse(element.Value);
          isPercent = true;
          break;

        case "IgnoreWeightFactorUpgrade":
        case "IgnoreDamageFactorUpgrade":
          value = -value;
          break;

        case "NeededAreaPercentUpgrade":
          isPercent = true;
          break;

        case "AdditionalHappiness":
          value = Int32.Parse(element.Value);
          break;

        case "ResolverUnitMovementSpeedUpgrade":
          this.Value = null;
          break;

        case "AccuracyUpgrade":
        case "LineOfSightRangeUpgrade":
        case "LoadingSpeedUpgrade":
        case "PublicServiceFullSatisfactionDistance":
        case "HealRadiusUpgrade":
        case "HealPerMinuteUpgrade":
        case "SpawnProbabilityFactor":
        case "SelfHealUpgrade":
        case "AttackRangeUpgrade":
        case "ForwardSpeedUpgrade":
        case "MaxHitpointsUpgrade":
        case "ResidentsUpgrade":
        case "StressUpgrade":
        case "ProvideElectricity":
        case "ModuleLimitUpgrade":
        case "NeedsElectricity":
        case "AttractivenessUpgrade":
        case "MaintenanceUpgrade":
        case "WorkforceAmountUpgrade":
        case "OutputAmountFactorUpgrade":
        case "ProductivityUpgrade":
        case "BlockBuyShare":
        case "BlockHostileTakeover":
        case "MaintainanceUpgrade":
        case "MoralePowerUpgrade":
          break;

        //case "MinPickupTimeUpgrade":
        //  this.Text = new Description("22219").InsertBefore("Minimum", "Minimum");
        //  break;
        //case "MaxPickupTimeUpgrade":
        //  this.Text = new Description("22219").InsertBefore("Maximum", "Maximum");
        //  break;
        //case "ScrapAmountLevelUpgrade":
        //  this.Text = new Description("22220");
        //  break;
        //case "RarityWeightUpgrade":
        //  this.Additionals = new List<Upgrade>();
        //  this.Text = new Description("22227");
        //  foreach (var item in element.Elements()) {
        //    if (item.Name.LocalName == "None") {
        //      this.Additionals.Add(new Upgrade() { Text = new Description("None", "None"), Value = $"+{item.Element("AdditionalWeight").Value}" });
        //    }
        //    else {
        //      this.Additionals.Add(new Upgrade() { Text = new Description(Assets.Descriptions[item.Name.LocalName]), Value = $"+{item.Element("AdditionalWeight").Value}" });
        //    }

        //  }
        //  break;
        //case "AllocationWeightUpgrade":
        //  this.Additionals = new List<Upgrade>();
        //  this.Text = new Description("22230");

        //  foreach (var item in element.Elements()) {
        //    if (item.Name.LocalName == "None") {
        //      this.Additionals.Add(new Upgrade() { Text = new Description("None", "None"), Value = $"+{item.Element("AdditionalWeight").Value}" });
        //    }
        //    else {
        //      this.Additionals.Add(new Upgrade() { Text = new Description(Assets.Descriptions[item.Name.LocalName]), Value = $"+{item.Element("AdditionalWeight").Value}" });

        //    }
        //  }
        //  break;

        default:
          throw new NotImplementedException(element.Name.LocalName);
      }
      if (value == null) {
        this.Value = String.Empty;
      }
      else {
        if (isPercent) {
          this.Value = value > 0 ? $"+{value * factor}%" : $"{value * factor}%";
        }
        else {
          this.Value = value > 0 ? $"+{value * factor}" : $"{value * factor}";
        }
      }
    }
    public Upgrade(String key, String amount) {
      var value = amount == null ? null : (Int32?)Int32.Parse(amount);
      this.Text = new Description(Assets.GetDescriptionID(key));
      switch (key) {
        case "PerkFormerPirate":
        case "PerkDiver":
        case "PerkZoologist":
        case "PerkMilitaryShip":
        case "PerkHypnotist":
        case "PerkAnthropologist":
        case "PerkPolyglot":
        case "PerkArcheologist":
        case "PerkMale":
        case "PerkFemale":
          value = null;
          this.Text = Text.InsertBefore("Trait: ", "Merkmal: ");
          break;

        default:
          break;
      }
      if (value == null) {
        this.Value = String.Empty;
      }
      else {
        this.Value = value.ToString();
      }
    }

    #endregion Constructors

    #region Methods

    public XElement ToXml() {
      var result = new XElement(this.GetType().Name);
      if (this.Text != null)
        result.Add(this.Text.ToXml("Text"));
      if (this.Value != null)
        result.Add(new XElement("Value", this.Value));
      if (this.AdditionalOutputs != null)
        result.Add(new XElement("AdditionalOutputs", this.AdditionalOutputs.Select(s => s.ToXml())));
      if (this.ReplaceInputs != null)
        result.Add(new XElement("ReplaceInputs", this.ReplaceInputs.Select(s => s.ToXml())));
      if (this.InputAmountUpgrades != null)
        result.Add(new XElement("InputAmountUpgrades", this.InputAmountUpgrades.Select(s => s.ToXml())));
      if (this.ReplacingWorkforce != null)
        result.Add(new XElement("ReplacingWorkforce", this.ReplacingWorkforce.ToXml()));
      if (this.Additionals != null)
        result.Add(new XElement("Additionals", this.Additionals.Select(s => s.ToXml())));
      return result;
    }

    #endregion Methods
  }
}