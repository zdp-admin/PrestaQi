namespace PrestaQi.Model.Spei
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.3190.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://h2h.integration.spei.enlacefi.lgec.com/")]
    public partial class ordenPagoWS : object, System.ComponentModel.INotifyPropertyChanged
    {

        private int causaDevolucionField;

        private bool causaDevolucionFieldSpecified;

        private string claveCatUsuario1Field;

        private string claveCatUsuario2Field;

        private string clavePagoField;

        private string claveRastreoField;

        private string claveRastreoDevolucionField;

        private string conceptoPagoField;

        private string conceptoPago2Field;

        private string cuentaBeneficiarioField;

        private string cuentaBeneficiario2Field;

        private string cuentaOrdenanteField;

        private int digitoIdentificadorBeneficiarioField;

        private bool digitoIdentificadorBeneficiarioFieldSpecified;

        private int digitoIdentificadorOrdenanteField;

        private bool digitoIdentificadorOrdenanteFieldSpecified;

        private string emailBeneficiarioField;

        private string empresaField;

        private string errorField;

        private string estadoField;

        private facturaWS[] facturasField;

        private string fechaLimitePagoField;

        private int fechaOperacionField;

        private bool fechaOperacionFieldSpecified;

        private string firmaField;

        private string folioOrigenField;

        private string folioPlataformaField;

        private string idClienteField;

        private int idEFField;

        private bool idEFFieldSpecified;

        private int institucionContraparteField;

        private bool institucionContraparteFieldSpecified;

        private int institucionOperanteField;

        private bool institucionOperanteFieldSpecified;

        private decimal ivaField;

        private bool ivaFieldSpecified;

        private int medioEntregaField;

        private bool medioEntregaFieldSpecified;

        private decimal montoField;

        private bool montoFieldSpecified;

        private decimal montoComisionField;

        private bool montoComisionFieldSpecified;

        private decimal montoInteresField;

        private bool montoInteresFieldSpecified;

        private decimal montoOriginalField;

        private bool montoOriginalFieldSpecified;

        private string nombreBeneficiarioField;

        private string nombreBeneficiario2Field;

        private string nombreCEPField;

        private string nombreOrdenanteField;

        private string numCelularBeneficiarioField;

        private string numCelularOrdenanteField;

        private int pagoComisionField;

        private bool pagoComisionFieldSpecified;

        private int prioridadField;

        private bool prioridadFieldSpecified;

        private string referenciaCobranzaField;

        private int referenciaNumericaField;

        private bool referenciaNumericaFieldSpecified;

        private int reintentosField;

        private bool reintentosFieldSpecified;

        private string rfcCEPField;

        private string rfcCurpBeneficiarioField;

        private string rfcCurpBeneficiario2Field;

        private string rfcCurpOrdenanteField;

        private string selloField;

        private string serieCertificadoField;

        private string swift1Field;

        private string swift2Field;

        private int tipoCuentaBeneficiarioField;

        private bool tipoCuentaBeneficiarioFieldSpecified;

        private int tipoCuentaBeneficiario2Field;

        private bool tipoCuentaBeneficiario2FieldSpecified;

        private int tipoCuentaOrdenanteField;

        private bool tipoCuentaOrdenanteFieldSpecified;

        private int tipoOperacionField;

        private bool tipoOperacionFieldSpecified;

        private int tipoPagoField;

        private bool tipoPagoFieldSpecified;

        private string topologiaField;

        private long tsAcuseBanxicoField;

        private bool tsAcuseBanxicoFieldSpecified;

        private long tsAcuseConfirmacionField;

        private bool tsAcuseConfirmacionFieldSpecified;

        private long tsCapturaField;

        private bool tsCapturaFieldSpecified;

        private long tsConfirmacionField;

        private bool tsConfirmacionFieldSpecified;

        private long tsDevolucionField;

        private bool tsDevolucionFieldSpecified;

        private long tsDevolucionRecibidaField;

        private bool tsDevolucionRecibidaFieldSpecified;

        private long tsEntregaField;

        private bool tsEntregaFieldSpecified;

        private long tsLiquidacionField;

        private bool tsLiquidacionFieldSpecified;

        private string uetrField;

        private string usuarioField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 0)]
        public int causaDevolucion
        {
            get
            {
                return this.causaDevolucionField;
            }
            set
            {
                this.causaDevolucionField = value;
                this.RaisePropertyChanged("causaDevolucion");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool causaDevolucionSpecified
        {
            get
            {
                return this.causaDevolucionFieldSpecified;
            }
            set
            {
                this.causaDevolucionFieldSpecified = value;
                this.RaisePropertyChanged("causaDevolucionSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 1)]
        public string claveCatUsuario1
        {
            get
            {
                return this.claveCatUsuario1Field;
            }
            set
            {
                this.claveCatUsuario1Field = value;
                this.RaisePropertyChanged("claveCatUsuario1");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 2)]
        public string claveCatUsuario2
        {
            get
            {
                return this.claveCatUsuario2Field;
            }
            set
            {
                this.claveCatUsuario2Field = value;
                this.RaisePropertyChanged("claveCatUsuario2");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 3)]
        public string clavePago
        {
            get
            {
                return this.clavePagoField;
            }
            set
            {
                this.clavePagoField = value;
                this.RaisePropertyChanged("clavePago");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 4)]
        public string claveRastreo
        {
            get
            {
                return this.claveRastreoField;
            }
            set
            {
                this.claveRastreoField = value;
                this.RaisePropertyChanged("claveRastreo");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 5)]
        public string claveRastreoDevolucion
        {
            get
            {
                return this.claveRastreoDevolucionField;
            }
            set
            {
                this.claveRastreoDevolucionField = value;
                this.RaisePropertyChanged("claveRastreoDevolucion");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 6)]
        public string conceptoPago
        {
            get
            {
                return this.conceptoPagoField;
            }
            set
            {
                this.conceptoPagoField = value;
                this.RaisePropertyChanged("conceptoPago");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 7)]
        public string conceptoPago2
        {
            get
            {
                return this.conceptoPago2Field;
            }
            set
            {
                this.conceptoPago2Field = value;
                this.RaisePropertyChanged("conceptoPago2");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 8)]
        public string cuentaBeneficiario
        {
            get
            {
                return this.cuentaBeneficiarioField;
            }
            set
            {
                this.cuentaBeneficiarioField = value;
                this.RaisePropertyChanged("cuentaBeneficiario");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 9)]
        public string cuentaBeneficiario2
        {
            get
            {
                return this.cuentaBeneficiario2Field;
            }
            set
            {
                this.cuentaBeneficiario2Field = value;
                this.RaisePropertyChanged("cuentaBeneficiario2");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 10)]
        public string cuentaOrdenante
        {
            get
            {
                return this.cuentaOrdenanteField;
            }
            set
            {
                this.cuentaOrdenanteField = value;
                this.RaisePropertyChanged("cuentaOrdenante");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 11)]
        public int digitoIdentificadorBeneficiario
        {
            get
            {
                return this.digitoIdentificadorBeneficiarioField;
            }
            set
            {
                this.digitoIdentificadorBeneficiarioField = value;
                this.RaisePropertyChanged("digitoIdentificadorBeneficiario");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool digitoIdentificadorBeneficiarioSpecified
        {
            get
            {
                return this.digitoIdentificadorBeneficiarioFieldSpecified;
            }
            set
            {
                this.digitoIdentificadorBeneficiarioFieldSpecified = value;
                this.RaisePropertyChanged("digitoIdentificadorBeneficiarioSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 12)]
        public int digitoIdentificadorOrdenante
        {
            get
            {
                return this.digitoIdentificadorOrdenanteField;
            }
            set
            {
                this.digitoIdentificadorOrdenanteField = value;
                this.RaisePropertyChanged("digitoIdentificadorOrdenante");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool digitoIdentificadorOrdenanteSpecified
        {
            get
            {
                return this.digitoIdentificadorOrdenanteFieldSpecified;
            }
            set
            {
                this.digitoIdentificadorOrdenanteFieldSpecified = value;
                this.RaisePropertyChanged("digitoIdentificadorOrdenanteSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 13)]
        public string emailBeneficiario
        {
            get
            {
                return this.emailBeneficiarioField;
            }
            set
            {
                this.emailBeneficiarioField = value;
                this.RaisePropertyChanged("emailBeneficiario");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 14)]
        public string empresa
        {
            get
            {
                return this.empresaField;
            }
            set
            {
                this.empresaField = value;
                this.RaisePropertyChanged("empresa");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 15)]
        public string error
        {
            get
            {
                return this.errorField;
            }
            set
            {
                this.errorField = value;
                this.RaisePropertyChanged("error");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 16)]
        public string estado
        {
            get
            {
                return this.estadoField;
            }
            set
            {
                this.estadoField = value;
                this.RaisePropertyChanged("estado");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 17)]
        [System.Xml.Serialization.XmlArrayItemAttribute("factura", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public facturaWS[] facturas
        {
            get
            {
                return this.facturasField;
            }
            set
            {
                this.facturasField = value;
                this.RaisePropertyChanged("facturas");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 18)]
        public string fechaLimitePago
        {
            get
            {
                return this.fechaLimitePagoField;
            }
            set
            {
                this.fechaLimitePagoField = value;
                this.RaisePropertyChanged("fechaLimitePago");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 19)]
        public int fechaOperacion
        {
            get
            {
                return this.fechaOperacionField;
            }
            set
            {
                this.fechaOperacionField = value;
                this.RaisePropertyChanged("fechaOperacion");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fechaOperacionSpecified
        {
            get
            {
                return this.fechaOperacionFieldSpecified;
            }
            set
            {
                this.fechaOperacionFieldSpecified = value;
                this.RaisePropertyChanged("fechaOperacionSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 20)]
        public string firma
        {
            get
            {
                return this.firmaField;
            }
            set
            {
                this.firmaField = value;
                this.RaisePropertyChanged("firma");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 21)]
        public string folioOrigen
        {
            get
            {
                return this.folioOrigenField;
            }
            set
            {
                this.folioOrigenField = value;
                this.RaisePropertyChanged("folioOrigen");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 22)]
        public string folioPlataforma
        {
            get
            {
                return this.folioPlataformaField;
            }
            set
            {
                this.folioPlataformaField = value;
                this.RaisePropertyChanged("folioPlataforma");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 23)]
        public string idCliente
        {
            get
            {
                return this.idClienteField;
            }
            set
            {
                this.idClienteField = value;
                this.RaisePropertyChanged("idCliente");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 24)]
        public int idEF
        {
            get
            {
                return this.idEFField;
            }
            set
            {
                this.idEFField = value;
                this.RaisePropertyChanged("idEF");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idEFSpecified
        {
            get
            {
                return this.idEFFieldSpecified;
            }
            set
            {
                this.idEFFieldSpecified = value;
                this.RaisePropertyChanged("idEFSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 25)]
        public int institucionContraparte
        {
            get
            {
                return this.institucionContraparteField;
            }
            set
            {
                this.institucionContraparteField = value;
                this.RaisePropertyChanged("institucionContraparte");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool institucionContraparteSpecified
        {
            get
            {
                return this.institucionContraparteFieldSpecified;
            }
            set
            {
                this.institucionContraparteFieldSpecified = value;
                this.RaisePropertyChanged("institucionContraparteSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 26)]
        public int institucionOperante
        {
            get
            {
                return this.institucionOperanteField;
            }
            set
            {
                this.institucionOperanteField = value;
                this.RaisePropertyChanged("institucionOperante");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool institucionOperanteSpecified
        {
            get
            {
                return this.institucionOperanteFieldSpecified;
            }
            set
            {
                this.institucionOperanteFieldSpecified = value;
                this.RaisePropertyChanged("institucionOperanteSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 27)]
        public decimal iva
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 28)]
        public int medioEntrega
        {
            get
            {
                return this.medioEntregaField;
            }
            set
            {
                this.medioEntregaField = value;
                this.RaisePropertyChanged("medioEntrega");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool medioEntregaSpecified
        {
            get
            {
                return this.medioEntregaFieldSpecified;
            }
            set
            {
                this.medioEntregaFieldSpecified = value;
                this.RaisePropertyChanged("medioEntregaSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 29)]
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

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 30)]
        public decimal montoComision
        {
            get
            {
                return this.montoComisionField;
            }
            set
            {
                this.montoComisionField = value;
                this.RaisePropertyChanged("montoComision");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool montoComisionSpecified
        {
            get
            {
                return this.montoComisionFieldSpecified;
            }
            set
            {
                this.montoComisionFieldSpecified = value;
                this.RaisePropertyChanged("montoComisionSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 31)]
        public decimal montoInteres
        {
            get
            {
                return this.montoInteresField;
            }
            set
            {
                this.montoInteresField = value;
                this.RaisePropertyChanged("montoInteres");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool montoInteresSpecified
        {
            get
            {
                return this.montoInteresFieldSpecified;
            }
            set
            {
                this.montoInteresFieldSpecified = value;
                this.RaisePropertyChanged("montoInteresSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 32)]
        public decimal montoOriginal
        {
            get
            {
                return this.montoOriginalField;
            }
            set
            {
                this.montoOriginalField = value;
                this.RaisePropertyChanged("montoOriginal");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool montoOriginalSpecified
        {
            get
            {
                return this.montoOriginalFieldSpecified;
            }
            set
            {
                this.montoOriginalFieldSpecified = value;
                this.RaisePropertyChanged("montoOriginalSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 33)]
        public string nombreBeneficiario
        {
            get
            {
                return this.nombreBeneficiarioField;
            }
            set
            {
                this.nombreBeneficiarioField = value;
                this.RaisePropertyChanged("nombreBeneficiario");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 34)]
        public string nombreBeneficiario2
        {
            get
            {
                return this.nombreBeneficiario2Field;
            }
            set
            {
                this.nombreBeneficiario2Field = value;
                this.RaisePropertyChanged("nombreBeneficiario2");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 35)]
        public string nombreCEP
        {
            get
            {
                return this.nombreCEPField;
            }
            set
            {
                this.nombreCEPField = value;
                this.RaisePropertyChanged("nombreCEP");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 36)]
        public string nombreOrdenante
        {
            get
            {
                return this.nombreOrdenanteField;
            }
            set
            {
                this.nombreOrdenanteField = value;
                this.RaisePropertyChanged("nombreOrdenante");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 37)]
        public string numCelularBeneficiario
        {
            get
            {
                return this.numCelularBeneficiarioField;
            }
            set
            {
                this.numCelularBeneficiarioField = value;
                this.RaisePropertyChanged("numCelularBeneficiario");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 38)]
        public string numCelularOrdenante
        {
            get
            {
                return this.numCelularOrdenanteField;
            }
            set
            {
                this.numCelularOrdenanteField = value;
                this.RaisePropertyChanged("numCelularOrdenante");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 39)]
        public int pagoComision
        {
            get
            {
                return this.pagoComisionField;
            }
            set
            {
                this.pagoComisionField = value;
                this.RaisePropertyChanged("pagoComision");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool pagoComisionSpecified
        {
            get
            {
                return this.pagoComisionFieldSpecified;
            }
            set
            {
                this.pagoComisionFieldSpecified = value;
                this.RaisePropertyChanged("pagoComisionSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 40)]
        public int prioridad
        {
            get
            {
                return this.prioridadField;
            }
            set
            {
                this.prioridadField = value;
                this.RaisePropertyChanged("prioridad");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool prioridadSpecified
        {
            get
            {
                return this.prioridadFieldSpecified;
            }
            set
            {
                this.prioridadFieldSpecified = value;
                this.RaisePropertyChanged("prioridadSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 41)]
        public string referenciaCobranza
        {
            get
            {
                return this.referenciaCobranzaField;
            }
            set
            {
                this.referenciaCobranzaField = value;
                this.RaisePropertyChanged("referenciaCobranza");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 42)]
        public int referenciaNumerica
        {
            get
            {
                return this.referenciaNumericaField;
            }
            set
            {
                this.referenciaNumericaField = value;
                this.RaisePropertyChanged("referenciaNumerica");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool referenciaNumericaSpecified
        {
            get
            {
                return this.referenciaNumericaFieldSpecified;
            }
            set
            {
                this.referenciaNumericaFieldSpecified = value;
                this.RaisePropertyChanged("referenciaNumericaSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 43)]
        public int reintentos
        {
            get
            {
                return this.reintentosField;
            }
            set
            {
                this.reintentosField = value;
                this.RaisePropertyChanged("reintentos");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool reintentosSpecified
        {
            get
            {
                return this.reintentosFieldSpecified;
            }
            set
            {
                this.reintentosFieldSpecified = value;
                this.RaisePropertyChanged("reintentosSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 44)]
        public string rfcCEP
        {
            get
            {
                return this.rfcCEPField;
            }
            set
            {
                this.rfcCEPField = value;
                this.RaisePropertyChanged("rfcCEP");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 45)]
        public string rfcCurpBeneficiario
        {
            get
            {
                return this.rfcCurpBeneficiarioField;
            }
            set
            {
                this.rfcCurpBeneficiarioField = value;
                this.RaisePropertyChanged("rfcCurpBeneficiario");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 46)]
        public string rfcCurpBeneficiario2
        {
            get
            {
                return this.rfcCurpBeneficiario2Field;
            }
            set
            {
                this.rfcCurpBeneficiario2Field = value;
                this.RaisePropertyChanged("rfcCurpBeneficiario2");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 47)]
        public string rfcCurpOrdenante
        {
            get
            {
                return this.rfcCurpOrdenanteField;
            }
            set
            {
                this.rfcCurpOrdenanteField = value;
                this.RaisePropertyChanged("rfcCurpOrdenante");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 48)]
        public string sello
        {
            get
            {
                return this.selloField;
            }
            set
            {
                this.selloField = value;
                this.RaisePropertyChanged("sello");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 49)]
        public string serieCertificado
        {
            get
            {
                return this.serieCertificadoField;
            }
            set
            {
                this.serieCertificadoField = value;
                this.RaisePropertyChanged("serieCertificado");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 50)]
        public string swift1
        {
            get
            {
                return this.swift1Field;
            }
            set
            {
                this.swift1Field = value;
                this.RaisePropertyChanged("swift1");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 51)]
        public string swift2
        {
            get
            {
                return this.swift2Field;
            }
            set
            {
                this.swift2Field = value;
                this.RaisePropertyChanged("swift2");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 52)]
        public int tipoCuentaBeneficiario
        {
            get
            {
                return this.tipoCuentaBeneficiarioField;
            }
            set
            {
                this.tipoCuentaBeneficiarioField = value;
                this.RaisePropertyChanged("tipoCuentaBeneficiario");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tipoCuentaBeneficiarioSpecified
        {
            get
            {
                return this.tipoCuentaBeneficiarioFieldSpecified;
            }
            set
            {
                this.tipoCuentaBeneficiarioFieldSpecified = value;
                this.RaisePropertyChanged("tipoCuentaBeneficiarioSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 53)]
        public int tipoCuentaBeneficiario2
        {
            get
            {
                return this.tipoCuentaBeneficiario2Field;
            }
            set
            {
                this.tipoCuentaBeneficiario2Field = value;
                this.RaisePropertyChanged("tipoCuentaBeneficiario2");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tipoCuentaBeneficiario2Specified
        {
            get
            {
                return this.tipoCuentaBeneficiario2FieldSpecified;
            }
            set
            {
                this.tipoCuentaBeneficiario2FieldSpecified = value;
                this.RaisePropertyChanged("tipoCuentaBeneficiario2Specified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 54)]
        public int tipoCuentaOrdenante
        {
            get
            {
                return this.tipoCuentaOrdenanteField;
            }
            set
            {
                this.tipoCuentaOrdenanteField = value;
                this.RaisePropertyChanged("tipoCuentaOrdenante");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tipoCuentaOrdenanteSpecified
        {
            get
            {
                return this.tipoCuentaOrdenanteFieldSpecified;
            }
            set
            {
                this.tipoCuentaOrdenanteFieldSpecified = value;
                this.RaisePropertyChanged("tipoCuentaOrdenanteSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 55)]
        public int tipoOperacion
        {
            get
            {
                return this.tipoOperacionField;
            }
            set
            {
                this.tipoOperacionField = value;
                this.RaisePropertyChanged("tipoOperacion");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tipoOperacionSpecified
        {
            get
            {
                return this.tipoOperacionFieldSpecified;
            }
            set
            {
                this.tipoOperacionFieldSpecified = value;
                this.RaisePropertyChanged("tipoOperacionSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 56)]
        public int tipoPago
        {
            get
            {
                return this.tipoPagoField;
            }
            set
            {
                this.tipoPagoField = value;
                this.RaisePropertyChanged("tipoPago");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tipoPagoSpecified
        {
            get
            {
                return this.tipoPagoFieldSpecified;
            }
            set
            {
                this.tipoPagoFieldSpecified = value;
                this.RaisePropertyChanged("tipoPagoSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 57)]
        public string topologia
        {
            get
            {
                return this.topologiaField;
            }
            set
            {
                this.topologiaField = value;
                this.RaisePropertyChanged("topologia");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 58)]
        public long tsAcuseBanxico
        {
            get
            {
                return this.tsAcuseBanxicoField;
            }
            set
            {
                this.tsAcuseBanxicoField = value;
                this.RaisePropertyChanged("tsAcuseBanxico");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tsAcuseBanxicoSpecified
        {
            get
            {
                return this.tsAcuseBanxicoFieldSpecified;
            }
            set
            {
                this.tsAcuseBanxicoFieldSpecified = value;
                this.RaisePropertyChanged("tsAcuseBanxicoSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 59)]
        public long tsAcuseConfirmacion
        {
            get
            {
                return this.tsAcuseConfirmacionField;
            }
            set
            {
                this.tsAcuseConfirmacionField = value;
                this.RaisePropertyChanged("tsAcuseConfirmacion");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tsAcuseConfirmacionSpecified
        {
            get
            {
                return this.tsAcuseConfirmacionFieldSpecified;
            }
            set
            {
                this.tsAcuseConfirmacionFieldSpecified = value;
                this.RaisePropertyChanged("tsAcuseConfirmacionSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 60)]
        public long tsCaptura
        {
            get
            {
                return this.tsCapturaField;
            }
            set
            {
                this.tsCapturaField = value;
                this.RaisePropertyChanged("tsCaptura");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tsCapturaSpecified
        {
            get
            {
                return this.tsCapturaFieldSpecified;
            }
            set
            {
                this.tsCapturaFieldSpecified = value;
                this.RaisePropertyChanged("tsCapturaSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 61)]
        public long tsConfirmacion
        {
            get
            {
                return this.tsConfirmacionField;
            }
            set
            {
                this.tsConfirmacionField = value;
                this.RaisePropertyChanged("tsConfirmacion");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tsConfirmacionSpecified
        {
            get
            {
                return this.tsConfirmacionFieldSpecified;
            }
            set
            {
                this.tsConfirmacionFieldSpecified = value;
                this.RaisePropertyChanged("tsConfirmacionSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 62)]
        public long tsDevolucion
        {
            get
            {
                return this.tsDevolucionField;
            }
            set
            {
                this.tsDevolucionField = value;
                this.RaisePropertyChanged("tsDevolucion");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tsDevolucionSpecified
        {
            get
            {
                return this.tsDevolucionFieldSpecified;
            }
            set
            {
                this.tsDevolucionFieldSpecified = value;
                this.RaisePropertyChanged("tsDevolucionSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 63)]
        public long tsDevolucionRecibida
        {
            get
            {
                return this.tsDevolucionRecibidaField;
            }
            set
            {
                this.tsDevolucionRecibidaField = value;
                this.RaisePropertyChanged("tsDevolucionRecibida");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tsDevolucionRecibidaSpecified
        {
            get
            {
                return this.tsDevolucionRecibidaFieldSpecified;
            }
            set
            {
                this.tsDevolucionRecibidaFieldSpecified = value;
                this.RaisePropertyChanged("tsDevolucionRecibidaSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 64)]
        public long tsEntrega
        {
            get
            {
                return this.tsEntregaField;
            }
            set
            {
                this.tsEntregaField = value;
                this.RaisePropertyChanged("tsEntrega");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tsEntregaSpecified
        {
            get
            {
                return this.tsEntregaFieldSpecified;
            }
            set
            {
                this.tsEntregaFieldSpecified = value;
                this.RaisePropertyChanged("tsEntregaSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 65)]
        public long tsLiquidacion
        {
            get
            {
                return this.tsLiquidacionField;
            }
            set
            {
                this.tsLiquidacionField = value;
                this.RaisePropertyChanged("tsLiquidacion");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tsLiquidacionSpecified
        {
            get
            {
                return this.tsLiquidacionFieldSpecified;
            }
            set
            {
                this.tsLiquidacionFieldSpecified = value;
                this.RaisePropertyChanged("tsLiquidacionSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 66)]
        public string uetr
        {
            get
            {
                return this.uetrField;
            }
            set
            {
                this.uetrField = value;
                this.RaisePropertyChanged("uetr");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 67)]
        public string usuario
        {
            get
            {
                return this.usuarioField;
            }
            set
            {
                this.usuarioField = value;
                this.RaisePropertyChanged("usuario");
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
