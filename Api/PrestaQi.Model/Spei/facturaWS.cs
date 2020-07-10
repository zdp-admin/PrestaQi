namespace PrestaQi.Model.Spei
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.3190.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://h2h.integration.spei.enlacefi.lgec.com/")]
    public partial class facturaWS : object, System.ComponentModel.INotifyPropertyChanged
    {

        private string folioField;

        private double ivaField;

        private bool ivaFieldSpecified;

        private decimal montoField;

        private bool montoFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 0)]
        public string folio
        {
            get
            {
                return this.folioField;
            }
            set
            {
                this.folioField = value;
                this.RaisePropertyChanged("folio");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 1)]
        public double iva
        {
            get
            {
                return this.ivaField;
            }
            set
            {
                this.ivaField = value;
                this.RaisePropertyChanged("iva");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ivaSpecified
        {
            get
            {
                return this.ivaFieldSpecified;
            }
            set
            {
                this.ivaFieldSpecified = value;
                this.RaisePropertyChanged("ivaSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 2)]
        public decimal monto
        {
            get
            {
                return this.montoField;
            }
            set
            {
                this.montoField = value;
                this.RaisePropertyChanged("monto");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool montoSpecified
        {
            get
            {
                return this.montoFieldSpecified;
            }
            set
            {
                this.montoFieldSpecified = value;
                this.RaisePropertyChanged("montoSpecified");
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
