// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Health.ItemTypes;
using Microsoft.HealthVault.Exceptions;
using Microsoft.HealthVault.Helpers;
using Microsoft.HealthVault.Thing;

namespace Microsoft.HealthVault.ItemTypes
{
    /// <summary>
    /// Represents a thing that encapsulates an asthma inhaler.
    /// </summary>
    ///
    /// <remarks>
    /// This class can represent any inhaler unit used to treat asthma. The
    /// inhaler may or may not have a device component to it. Each new canister
    /// should be represented by a new <see cref="AsthmaInhaler"/> instance, even
    /// if there is a containing device that is reusable. Changes in regimen
    /// (minimum/maximum doses per day) should also cause a new health record
    /// item to be created.
    /// </remarks>
    ///
    public class AsthmaInhaler : ThingBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AsthmaInhaler"/> class with
        /// default values.
        /// </summary>
        ///
        /// <remarks>
        /// The item is not added to the health record until the
        /// <see cref="HealthRecordAccessor.NewItemAsync(ThingBase)"/> method
        /// is called.
        /// </remarks>
        ///
        public AsthmaInhaler()
            : base(TypeId)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AsthmaInhaler"/> class
        /// specifying the mandatory values.
        /// </summary>
        ///
        /// <param name="startDate">
        /// The approximate date and time when the inhaler use began.
        /// </param>
        ///
        /// <param name="drug">
        /// The name of the drug being used in the inhaler.
        /// </param>
        ///
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="startDate"/> parameter is <b>null</b>.
        /// </exception>
        ///
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="drug"/> parameter is <b>null</b>.
        /// </exception>
        ///
        public AsthmaInhaler(ApproximateDateTime startDate, CodableValue drug)
            : base(TypeId)
        {
            this.StartDate = startDate;
            this.Drug = drug;
        }

        /// <summary>
        /// Retrieves the unique identifier for the item type.
        /// </summary>
        ///
        public static new readonly Guid TypeId =
            new Guid("ff9ce191-2096-47d8-9300-5469a9883746");

        /// <summary>
        /// Populates this <see cref="AsthmaInhaler"/> instance from the data in the XML.
        /// </summary>
        ///
        /// <param name="typeSpecificXml">
        /// The XML to get the asthma inhaler data from.
        /// </param>
        ///
        /// <exception cref="InvalidOperationException">
        /// The first node in the <paramref name="typeSpecificXml"/>
        /// parameter is not an asthma-inhaler node.
        /// </exception>
        ///
        protected override void ParseXml(IXPathNavigable typeSpecificXml)
        {
            XPathNavigator inhalerNav =
                typeSpecificXml.CreateNavigator().SelectSingleNode(
                    "asthma-inhaler");

            Validator.ThrowInvalidIfNull(inhalerNav, Resources.AsthmaInhalerUnexpectedNode);

            this.drug = new CodableValue();
            this.drug.ParseXml(inhalerNav.SelectSingleNode("drug"));

            XPathNavigator strengthNav =
                inhalerNav.SelectSingleNode("strength");

            if (strengthNav != null)
            {
                this.strength = new CodableValue();
                this.strength.ParseXml(strengthNav);
            }

            XPathNavigator purposeNav =
                inhalerNav.SelectSingleNode("purpose");

            if (purposeNav != null)
            {
                try
                {
                    this.purpose =
                        (InhalerPurpose)Enum.Parse(
                            typeof(InhalerPurpose),
                            purposeNav.Value,
                            true);
                }
                catch (ArgumentException)
                {
                    this.purpose = InhalerPurpose.None;
                    this.purposeString = purposeNav.Value;
                }
            }

            this.startDate = new ApproximateDateTime();
            this.startDate.ParseXml(inhalerNav.SelectSingleNode("start-date"));

            XPathNavigator stopDateNav =
                inhalerNav.SelectSingleNode("stop-date");

            if (stopDateNav != null)
            {
                this.stopDate = new ApproximateDateTime();
                this.stopDate.ParseXml(stopDateNav);
            }

            XPathNavigator expirationDateNav =
                inhalerNav.SelectSingleNode("expiration-date");

            if (expirationDateNav != null)
            {
                this.expirationDate = new ApproximateDateTime();
                this.expirationDate.ParseXml(expirationDateNav);
            }

            XPathNavigator deviceIdNav =
                inhalerNav.SelectSingleNode("device-id");

            if (deviceIdNav != null)
            {
                this.deviceId = deviceIdNav.Value;
            }

            XPathNavigator initialDosesNav =
                inhalerNav.SelectSingleNode("initial-doses");

            if (initialDosesNav != null)
            {
                this.initialDoses = initialDosesNav.ValueAsInt;
            }

            XPathNavigator minDosesNav =
                inhalerNav.SelectSingleNode("min-daily-doses");

            if (minDosesNav != null)
            {
                this.minDailyDoses = minDosesNav.ValueAsInt;
            }

            XPathNavigator maxDosesNav =
                inhalerNav.SelectSingleNode("max-daily-doses");

            if (maxDosesNav != null)
            {
                this.maxDailyDoses = maxDosesNav.ValueAsInt;
            }

            XPathNavigator canAlertNav =
                inhalerNav.SelectSingleNode("can-alert");

            if (canAlertNav != null)
            {
                this.canAlert = canAlertNav.ValueAsBoolean;
            }

            XPathNodeIterator alertIterator =
                inhalerNav.Select("alert");

            foreach (XPathNavigator alertNav in alertIterator)
            {
                Alert alert = new Alert();
                alert.ParseXml(alertNav);
                this.alerts.Add(alert);
            }
        }

        /// <summary>
        /// Writes the asthma inhaler data to the specified XmlWriter.
        /// </summary>
        ///
        /// <param name="writer">
        /// The XmlWriter to write the asthma inhaler data to.
        /// </param>
        ///
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="writer"/> is <b>null</b>.
        /// </exception>
        ///
        /// <exception cref="ThingSerializationException">
        /// If <see cref="StartDate"/> is <b>null</b>, or
        /// <see cref="Drug"/> is <b>null</b> or empty.
        /// </exception>
        ///
        public override void WriteXml(XmlWriter writer)
        {
            Validator.ThrowIfWriterNull(writer);
            Validator.ThrowSerializationIfNull(this.startDate, Resources.AsthmaInhalerStartDateNotSet);
            Validator.ThrowSerializationIfNull(this.drug, Resources.AsthmaInhalerDrugNotSet);

            // <asthma-inhaler>
            writer.WriteStartElement("asthma-inhaler");

            // <drug>
            this.drug.WriteXml("drug", writer);

            // <strength>
            this.strength?.WriteXml("strength", writer);

            if (this.purpose != InhalerPurpose.None)
            {
                // <purpose>
                writer.WriteElementString("purpose", this.purpose.ToString());
            }
            else
            {
                if (!string.IsNullOrEmpty(this.purposeString))
                {
                    // <purpose>
                    writer.WriteElementString("purpose", this.purposeString);
                }
            }

            // <start-date>
            this.startDate.WriteXml("start-date", writer);

            if (this.stopDate != null)
            {
                // <stop-date>
                this.stopDate.WriteXml("stop-date", writer);
            }

            if (this.expirationDate != null)
            {
                // <expiration-date>
                this.expirationDate.WriteXml("expiration-date", writer);
            }

            if (!string.IsNullOrEmpty(this.deviceId))
            {
                // <device-id>
                writer.WriteElementString("device-id", this.deviceId);
            }

            if (this.initialDoses != null)
            {
                // <initial-doses>
                writer.WriteElementString(
                    "initial-doses",
                    ((int)this.initialDoses).ToString(CultureInfo.InvariantCulture));
            }

            if (this.minDailyDoses != null)
            {
                // <initial-doses>
                writer.WriteElementString(
                    "min-daily-doses",
                    ((int)this.minDailyDoses).ToString(CultureInfo.InvariantCulture));
            }

            if (this.maxDailyDoses != null)
            {
                // <max-daily-doses>
                writer.WriteElementString(
                    "max-daily-doses",
                    ((int)this.maxDailyDoses).ToString(CultureInfo.InvariantCulture));
            }

            if (this.canAlert != null)
            {
                writer.WriteElementString(
                    "can-alert",
                    SDKHelper.XmlFromBool((bool)this.canAlert));
            }

            foreach (Alert alert in this.alerts)
            {
                alert.WriteXml("alert", writer);
            }

            // </asthma-inhaler>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Gets or sets the date and time when the inhaler use began.
        /// </summary>
        ///
        /// <returns>
        /// An <see cref="ApproximateDateTime"/> instance representing the date
        /// and time.
        /// </returns>
        ///
        /// <remarks>
        /// The value defaults to the current year, month, and day.
        /// </remarks>
        ///
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="value"/> is <b>null</b>.
        /// </exception>
        ///
        public ApproximateDateTime StartDate
        {
            get { return this.startDate; }

            set
            {
                Validator.ThrowIfArgumentNull(value, nameof(this.StartDate), Resources.AsthmaInhalerStartDateMandatory);
                this.startDate = value;
            }
        }

        private ApproximateDateTime startDate = new ApproximateDateTime();

        /// <summary>
        /// Gets or sets the drug being used in the inhaler.
        /// </summary>
        ///
        /// <returns>
        /// A <see cref="CodableValue"/> instance representing the drug.
        /// </returns>
        ///
        /// <remarks>
        /// The name of the drug in the canister.
        /// </remarks>
        ///
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="value"/> is <b>null</b>.
        /// </exception>
        ///
        public CodableValue Drug
        {
            get { return this.drug; }

            set
            {
                Validator.ThrowIfArgumentNull(value, nameof(this.Drug), Resources.AsthmaInhalerUseDrugMandatory);
                this.drug = value;
            }
        }

        private CodableValue drug;

        /// <summary>
        /// Gets or sets the textual description of the drug strength
        /// (e.g., '44 mcg / puff').
        /// </summary>
        ///
        /// <returns>
        /// A <see cref="CodableValue"/> instance representing the description.
        /// </returns>
        ///
        /// <remarks>
        /// Set the value to <b>null</b> if the strength should not be stored.
        /// </remarks>
        ///
        public CodableValue Strength
        {
            get { return this.strength; }
            set { this.strength = value; }
        }

        private CodableValue strength;

        /// <summary>
        /// Gets or sets the count of doses for each inhaler use.
        /// </summary>
        ///
        /// <returns>
        /// An <see cref="InhalerPurpose"/> instance representing the count.
        /// </returns>
        ///
        /// <remarks>
        /// Set the value to <see cref="InhalerPurpose.None"/> if the purpose should
        /// not be stored.
        /// </remarks>
        ///
        public InhalerPurpose Purpose
        {
            get { return this.purpose; }
            set { this.purpose = value; }
        }

        private InhalerPurpose purpose;
        private string purposeString;

        /// <summary>
        /// Gets or sets the date and time when the inhaler was retired.
        /// </summary>
        ///
        /// <returns>
        /// An <see cref="ApproximateDateTime"/> instance representing the date
        /// and time.
        /// </returns>
        ///
        /// <remarks>
        /// The value defaults to the current year, month, and day.
        /// <br/><br/>
        /// Set the value to <b>null</b> if the stop date should not be stored or
        /// is still in use.
        /// </remarks>
        ///
        public ApproximateDateTime StopDate
        {
            get { return this.stopDate; }
            set { this.stopDate = value; }
        }

        private ApproximateDateTime stopDate = new ApproximateDateTime();

        /// <summary>
        /// Gets or sets the date and time when the canister has clinically
        /// expired.
        /// </summary>
        ///
        /// <returns>
        /// An <see cref="ApproximateDateTime"/> instance representing the date
        /// and time.
        /// </returns>
        ///
        /// <remarks>
        /// The value defaults to the current year, month, and day.
        /// <br/><br/>
        /// Set the value to <b>null</b> if the expiration date should not be stored.
        /// </remarks>
        ///
        public ApproximateDateTime ExpirationDate
        {
            get { return this.expirationDate; }
            set { this.expirationDate = value; }
        }

        private ApproximateDateTime expirationDate = new ApproximateDateTime();

        /// <summary>
        /// Gets or sets the identifier for the device.
        /// </summary>
        ///
        /// <returns>
        /// A string representing the identifier.
        /// </returns>
        ///
        /// <remarks>
        /// Set the value to <b>null</b> if the device identifier should not be
        /// stored.
        /// </remarks>
        ///
        /// <exception cref="ArgumentException">
        /// If <paramref name="value"/> contains only whitespace.
        /// </exception>
        ///
        public string DeviceId
        {
            get { return this.deviceId; }

            set
            {
                Validator.ThrowIfStringIsWhitespace(value, "DeviceId");
                this.deviceId = value;
            }
        }

        private string deviceId;

        /// <summary>
        /// Gets or sets the number of doses in the unit at the time of
        /// item creation.
        /// </summary>
        ///
        /// <returns>
        /// An integer representing the number of doses.
        /// </returns>
        ///
        /// <remarks>
        /// The number of doses in the unit may not correspond to the number
        /// of doses the canister started with, since the expectation is that
        /// a change in regimen will cause a new item to be created as well.
        /// <br/><br/>
        /// Set the value to <b>null</b> if the initial doses should not be stored.
        /// </remarks>
        ///
        public int? InitialDoses
        {
            get { return this.initialDoses; }
            set { this.initialDoses = value; }
        }

        private int? initialDoses;

        /// <summary>
        /// Gets or sets the minimum number of doses that should be taken
        /// each day.
        /// </summary>
        ///
        /// <returns>
        /// An integer representing the minimum number of doses.
        /// </returns>
        ///
        /// <remarks>
        /// Set the value to <b>null</b> if the minimum doses should not be stored.
        /// </remarks>
        ///
        public int? MinimumDailyDoses
        {
            get { return this.minDailyDoses; }
            set { this.minDailyDoses = value; }
        }

        private int? minDailyDoses;

        /// <summary>
        /// Gets or sets the maximum number of doses that should be taken
        /// each day.
        /// </summary>
        ///
        /// <returns>
        /// An integer representing the maximum number of doses.
        /// </returns>
        ///
        /// <remarks>
        /// Set the value to <b>null</b> if the maximum doses should not be stored.
        /// </remarks>
        ///
        public int? MaximumDailyDoses
        {
            get { return this.maxDailyDoses; }
            set { this.maxDailyDoses = value; }
        }

        private int? maxDailyDoses;

        /// <summary>
        /// Gets or sets a value indicating whether the inhaler can show alerts.
        /// </summary>
        ///
        /// <returns>
        /// <b>true</b> if the inhaler can show alerts; otherwise, <b>false</b>.
        /// If <b>null</b>, it is unknown whether the inhaler
        /// can show alerts.
        /// </returns>
        ///
        public bool? CanAlert
        {
            get { return this.canAlert; }
            set { this.canAlert = value; }
        }

        private bool? canAlert;

        /// <summary>
        /// Gets a collection of alerts for the inhaler.
        /// </summary>
        ///
        /// <returns>
        /// A collection of alerts.
        /// </returns>
        ///
        /// <remarks>
        /// To add an alert, pass the Add method of the returned collection
        /// an instance of the <see cref="Alert"/> class.
        /// </remarks>
        ///
        public Collection<Alert> Alerts => this.alerts;

        private readonly Collection<Alert> alerts = new Collection<Alert>();

        /// <summary>
        /// Gets a string representation of the asthma inhaler item.
        /// </summary>
        ///
        /// <returns>
        /// A string representation of the asthma inhaler item.
        /// </returns>
        ///
        public override string ToString()
        {
            if (this.Drug != null)
            {
                return this.Drug.Text;
            }

            return string.Empty;
        }
    }
}
