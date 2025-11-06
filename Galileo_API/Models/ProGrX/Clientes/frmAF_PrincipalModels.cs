namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AfCatalogosGeneralesDto
    {
        public List<DropDownListaGenericaModel>? EstadoCivil { get; set; }
        public List<DropDownListaGenericaModel>? Divisas { get; set; }
        public List<DropDownListaGenericaModel>? TiposIdentificacion { get; set; }
        public List<DropDownListaGenericaModel>? Profesiones { get; set; }
        public List<DropDownListaGenericaModel>? Sectores { get; set; }
        public List<DropDownListaGenericaModel>? Sociedades { get; set; }
        public List<DropDownListaGenericaModel>? ActividadesEconomicas { get; set; }
        public List<DropDownListaGenericaModel>? Paises { get; set; }
        public List<DropDownListaGenericaModel>? EstadosPersonaIngreso { get; set; }
        public List<DropDownListaGenericaModel>? Nacionalidades { get; set; }
        public List<DropDownListaGenericaModel>? NivelAcademico { get; set; }
        public List<DropDownListaGenericaModel>? EstadoLaboral { get; set; }
        public List<DropDownListaGenericaModel>? ActividadLaboral { get; set; }
        public List<DropDownListaGenericaModel>? RelacionParentesco { get; set; }
        public List<DropDownListaGenericaModel>? Promotores { get; set; }
        public List<DropDownListaGenericaModel>? Instituciones { get; set; }
        public List<DropDownListaGenericaModel>? Deductoras { get; set; }
        public List<DropDownListaGenericaModel>? Departamentos { get; set; }
        public List<DropDownListaGenericaModel>? Secciones { get; set; }
        public List<DropDownListaGenericaModel>? Actividades { get; set; }
        public List<DropDownListaGenericaModel>? Unidad { get; set; }
    }

    public class AfPersonaDto
    {
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public string? Provincia { get; set; }
        public string? Canton { get; set; }
        public string? Distrito { get; set; }
        public string? Direccion { get; set; }
        public DateTime? Fecha_Nac { get; set; }
        public string? Sexo { get; set; }
        public string? EstadoCivil { get; set; }
        public int? Hijos { get; set; }
        public string? EstadoLaboral { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public string? EstadoActual { get; set; }
        public string? Apto { get; set; }
        public string? Af_Email { get; set; }
        public string? Notas { get; set; }
        public string? UltimoEstado { get; set; }
        public int? Ind_Liquidacion { get; set; }
        public string? Cod_Banco { get; set; }
        public string? Cuenta_Ahorros { get; set; }
        public string? Cod_Departamento { get; set; }
        public string? Cod_Institucion { get; set; }
        public string? Cod_Seccion { get; set; }
        public DateTime? ActualizaFecha { get; set; }
        public string? ActualizaUser { get; set; }
        public int? Bloqueo { get; set; }
        public string? Id_Promotor { get; set; }
        public string? Cod_Profesion { get; set; }
        public string? Cod_Sector { get; set; }
        public string? Boleta { get; set; }
        public string? CedulaR { get; set; }
        public int? Af_NPagos { get; set; }
        public string? EstadoActa { get; set; }
        public string? NActa { get; set; }
        public DateTime? FecActa { get; set; }
        public string? Congelar { get; set; }
        public string? prideduc { get; set; }
        public string? Pin { get; set; }
        public int? Ind_SinAporte { get; set; }
        public string? Reg_User { get; set; }
        public DateTime? Reg_Fecha { get; set; }
        public string? Id_Boleta_AF { get; set; }
        public string? Nota_User { get; set; }
        public DateTime? Nota_Fecha { get; set; }
        public DateTime? Fecha_Comision { get; set; }
        public int? Ind_Doble_Deduccion { get; set; }
        public string? Tipo_ID { get; set; }
        public string? Conyuge_Cedula { get; set; }
        public string? Conyuge_Nombre { get; set; }
        public string? Conyuge_TelTra { get; set; }
        public string? Conyuge_TelTraExt { get; set; }
        public string? Conyuge_TelCell { get; set; }
        public string? Notificaciones { get; set; }
        public DateTime? Nombramiento_Fecha { get; set; }
        public string? Albacea_Cedula { get; set; }
        public string? Albacea_Nombre { get; set; }
        public string? Cod_Oficina { get; set; }
        public string? Profesion { get; set; }
        public int? Comision_Autoriza { get; set; }
        public string? Razon_Social { get; set; }
        public string? Cod_Actividad { get; set; }
        public string? Cod_Sociedad { get; set; }
        public int? Ind_Propiedades { get; set; }
        public string? Autoriza_Comision_Notas { get; set; }
        public DateTime? Ben_Update_Fecha { get; set; }
        public string? Ben_Update_Usuario { get; set; }
        public string? Credito_CLS_Tipo { get; set; }
        public DateTime? Credito_CLS_Fecha { get; set; }
        public string? Consentimiento_Contacto_Usuario { get; set; }
        public DateTime? Consentimiento_Contacto_Fecha { get; set; }
        public int? Cliente_VIP { get; set; }
        public string? Email_02 { get; set; }
        public string? Facebook { get; set; }
        public string? LinkedIn { get; set; }
        public string? Instagram { get; set; }
        public string? Blog { get; set; }
        public string? Twitter { get; set; }
        public string? dimex_cedula { get; set; }
        public string? Crd_Categoria { get; set; }
        public DateTime? Crd_Categoria_Fecha { get; set; }
        public string? Empleado_ID { get; set; }
        public string? Cod_Nacionalidad { get; set; }
        public string? Cod_Deductora { get; set; }
        public string? dimex_usuario { get; set; }
        public DateTime? dimex_fecha { get; set; }
        public int? dimex_Activo { get; set; }
        public string? Salario_Usuario { get; set; }
        public DateTime? Salario_Fecha { get; set; }
        public decimal? Salario_Monto { get; set; }
        public string? Salario_Divisa { get; set; }
        public string? Tramite_Resolucion_Usuario { get; set; }
        public DateTime? Tramite_Resolucion_Fecha { get; set; }
        public string? Tramite_Resolucion_Nota { get; set; }
        public DateTime? dimex_actualiza_fecha { get; set; }
        public string? dimex_actualiza_usuario { get; set; }
        public int? Sorteo_Acciones { get; set; }
        public string? Sorteo_Acciones_Prov { get; set; }
        public string? UP { get; set; }
        public string? UT { get; set; }
        public string? CT { get; set; }
        public int? I_Beneficiarios { get; set; }
        public int? I_Trabajo_Propio { get; set; }
        public string? Tipo_Patron { get; set; }
        public string? Cod_Cargo { get; set; }
        public int? pep_ind { get; set; }
        public DateTime? PEP_Inicio { get; set; }
        public DateTime? PEP_Corte { get; set; }
        public string? pep_cargo { get; set; }
        public string? tipo_ces { get; set; }
        public string? Cod_Pais_Nac { get; set; }
        public int? Ind_Activo { get; set; }
        public DateTime? Fecha_Inactiva { get; set; }
        public DateTime? Fecha_Activa { get; set; }
        public string? EmailSecundario { get; set; }
        public string? nombrev2 { get; set; }
        public string? Apellido1 { get; set; }
        public string? Apellido2 { get; set; }
        public DateTime? fec_venc_ced { get; set; }
        public string? Nivel_Academico { get; set; }
        public string? Inscrito_SUGEF { get; set; }
        public string? Productos { get; set; }
        public string? Actividades { get; set; }
        public string? Rango { get; set; }
        public string? Direccion_Adicional { get; set; }
        public DateTime? Fecha_Ult_Carga_Act_Datos { get; set; }
        public string? Usuario_Carga_Act_Datos { get; set; }
        public string? Proyeccion_Social { get; set; }
        public int? AutorizaAdminAportePatronal { get; set; }
        public string? Albacea_TelTra { get; set; }
        public string? Albacea_TelTraExt { get; set; }
        public string? Albacea_TelCell { get; set; }
        public string? Id_Persona { get; set; }
        public string? Tra_Provincia { get; set; }
        public string? Tra_Canton { get; set; }
        public string? Tra_Distrito { get; set; }
        public string? Tra_Direccion { get; set; }
        public string? EstadoPersonaDesc { get; set; }
        public string? EstadoPersona { get; set; }
        public string? InstitucionDesc { get; set; }
        public string? DepartamentoDesc { get; set; }
        public string? SeccionDesc { get; set; }
        public string? Promotor { get; set; }
        public string? ProfesionDesc { get; set; }
        public string? SectorDesc { get; set; }
        public int? AnioServicio { get; set; }
        public string? ProvinciaDesc { get; set; }
        public string? CantonDesc { get; set; }
        public string? DistritoDesc { get; set; }
        public string? TipoIdDesc { get; set; }
        public string? Tipo_Personeria { get; set; }
        public string? SociedadDesc { get; set; }
        public string? ActividadDesc { get; set; }
        public string? OficinaDesc { get; set; }
        public string? Pais { get; set; }
        public string? Nacionalidad { get; set; }
        public string? EstadoCivilDesc { get; set; }
        public string? DeductoraDesc { get; set; }
        public string? DeductoraCod { get; set; }
        public string? EstadoLaboralDesc { get; set; }
        public string? NivelAcademicoDesc { get; set; }
        public string? UP_Desc { get; set; }
        public string? ct_desc { get; set; }
        public string? UT_Desc { get; set; }
        public string? SalarioDivisaDesc { get; set; }
        public decimal? salarioEmbargo { get; set; }
        public decimal? Salario_Devengado { get; set; }
        public decimal? Salario_Neto { get; set; }
        public decimal? Salario_Rebajos { get; set; }
        public string? Salario_Tipo { get; set; }
        public string? Tipo_Salario { get; set; }
        public string? c_actividadDesc { get; set; }
        public string? Tra_Provincia_Desc { get; set; }
        public string? Tra_Canton_Desc { get; set; }
        public string? Tra_Distrito_Desc { get; set; }
    }

    public class AfPersonaProductoDto
    {
        public int Codigo { get; set; }
        public string? Descripcion { get; set; }
        public bool Asignado { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
    }

    public class AfPersonaRelacionDto
    {
        public int? pr_id { get; set; }
        public string? cedulasocio { get; set; }
        public string? cedula { get; set; }
        public string? nombre_completo { get; set; }
        public string? apellido1 { get; set; }
        public string? apellido2 { get; set; }
        public string? nombre { get; set; }
        public bool? empleado { get; set; }
        public string? teltra { get; set; }
        public string? teltraext { get; set; }
        public string? telcell { get; set; }
        public string? tipo_relacion { get; set; }
        public string? tipo_relacion_desc { get; set; }
        public string? tipo_id { get; set; }
        public string? tipo_id_desc { get; set; }
        public int? activo { get; set; }                 // Si es BIT en SQL y preferís bool, cambiá a bool?
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
    }

    public class AfPersonaEmailDto
    {
        public int? ID_EMAIL { get; set; }
        public string? CEDULA { get; set; }
        public string? EMAIL { get; set; }
        public bool PRINCIPAL { get; set; }
        public int? IdTipoEmail { get; set; }
        public string? TipoDesc { get; set; }
        public DateTime? AGREGA_FECHA { get; set; }
        public string? AGREGA_USUARIO { get; set; }
        public DateTime? MODIFICA_FECHA { get; set; }
        public string? MODIFICA_USUARIO { get; set; }
    }

    public class AfPersonaMovimientoDto
    {
        public string? COD_MOVIMIENTO { get; set; }
    }

    public class AfPersonaTelefonoDto
    {
        public int? Telefono { get; set; }
        public int? Tipo { get; set; }
        public string? Numero { get; set; }
        public string? Ext { get; set; }
        public string? Contacto { get; set; }
        public string? Usuario { get; set; }
        public DateTime? Fecha { get; set; }
        public string? TipoDesc { get; set; }
        public DateTime? FechaServidor { get; set; }
    }

    public class AfPersonaBeneficiarioDto
    {
        public int linea_Id { get; set; }
        public string? cedula { get; set; }
        public string? tipo_Relacion { get; set; }
        public string? cedula_Beneficiario { get; set; }
        public string? nombre { get; set; }
        public DateTime? fecha_Nac { get; set; }
        public string? cod_Parentesco { get; set; }
        public decimal? porcentaje { get; set; }
        public string? telefono1 { get; set; }
        public string? telefono2 { get; set; }
        public string? notas { get; set; }
        public string? email { get; set; }
        public string? apto_Postal { get; set; }
        public string? direccion { get; set; }
        public bool? aplica_Seguros { get; set; }
        public string? registro_Usuario { get; set; }
        public DateTime? registro_Fecha { get; set; }
        public string? sexo { get; set; }
        public bool? albacea_Ind { get; set; }
        public string? albacea_Cedula { get; set; }
        public string? albacea_Nombre { get; set; }
        public string? albacea_Movil { get; set; }
        public string? albacea_Teltra { get; set; }
        public string? albacea_Teltra_Ext { get; set; }
        public int? tipo_Id { get; set; }
        public bool? albacea_Check { get; set; }
        public string? parentesco { get; set; }
        public string? relacion_Desc { get; set; }
        public int? tipo_Id_R { get; set; }
        public string? tipo_Id_Desc { get; set; }
    }

    public class AfPersonaCuentaBancariaDto
    {
        public string? Banco { get; set; }
        public string? TipoDesc { get; set; }
        public string? cod_Divisa { get; set; }
        public string? CUENTA_INTERNA { get; set; }
        public bool? CUENTA_INTERBANCA { get; set; }
        public bool? ACTIVA { get; set; }
        public string? DESTINO { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string? REGISTRO_USUARIO { get; set; }
    }

    public class AfPersonaNombramientoDto
    {
        public int ID_LINEA { get; set; }
        public string? cedula { get; set; }
        public string? estado_laboral { get; set; }
        public DateTime? fecha { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public string? EstadoLaboralDesc { get; set; }
    }

    public class AfPersonaSalarioDto
    {
        public int LINEA_ID { get; set; }
        public string? CEDULA { get; set; }
        public string? cod_divisa { get; set; }
        public string? TIPO_SALARIO { get; set; }
        public DateTime? fecha_salario { get; set; }
        public decimal? salario_devengado { get; set; }
        public decimal? rebajos_total { get; set; }
        public decimal? salario_neto { get; set; }
        public string? EMBARGO { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public string? tiposalariodesc { get; set; }
    }

    public class AfPersonaDireccionDto
    {
        public string? CEDULA { get; set; }
        public int? LINEA_ID { get; set; }
        public string? PROVINCIA { get; set; }
        public string? CANTON { get; set; }
        public string? DISTRITO { get; set; }
        public string? DIRECCION { get; set; }
        public string? EMAIL_01 { get; set; }
        public string? EMAIL_02 { get; set; }
        public string? TELEFONO_01 { get; set; }
        public string? TELEFONO_02 { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string? REGISTRO_USUARIO { get; set; }
        public string? COD_APP { get; set; }
        public bool? SINCRONIZADA { get; set; }
        public string? APROBADA_USUARIO { get; set; }
        public DateTime? APROBADA_FECHA { get; set; }
        public string? APROBADA_ESTADO { get; set; }
        public string? ProvinciaDesc { get; set; }
        public string? CantonDesc { get; set; }
        public string? DistritoDesc { get; set; }
        public int? Tipo { get; set; }
        public string? TipoDesc { get; set; }
    }

    public class AfPersonaIngresoDto
    {
        public int? CONSEC { get; set; }
        public string? CEDULA { get; set; }
        public string? fecha_ingreso { get; set; }
        public int? ID_PROMOTOR { get; set; }
        public string? BOLETA { get; set; }
        public string? USUARIO { get; set; }
        public DateTime? FECHA { get; set; }
        public string? COD_OFICINA { get; set; }
        public string? COD_REMESA { get; set; }
        public string? ANALISTA_REVISION { get; set; }
        public string? ANALISTA_RECEPCION { get; set; }
        public string? ESTADO { get; set; }
        public bool? AFILIACION_DIGITAL { get; set; }
        public bool? ENVIADO_ARCHIVO { get; set; }
        public string? TIPO_ING { get; set; }
        public string? Promotor { get; set; }
        public string? Tipo_Desc { get; set; }
    }

    public class AfPersonaRenunciaDto
    {
        public DateTime? Fecha { get; set; }
        public string? Tipo { get; set; }
        public string? Descripcion { get; set; }
    }

    public class AfPersonaLiquidacionDto
    {
        public int Consec { get; set; }
        public DateTime? Fecliq { get; set; }
        public string? EstadoActliq { get; set; }
    }

    public class AfPersonaTarjetaDto
    {
        public string? CEDULA { get; set; }
        public string? TARJETA_NUMERO { get; set; }
        public string? TARJETA_TIPO { get; set; }
        public string? TARJETA_MASK { get; set; }
        public string? TARJETA_CODE { get; set; }
        public DateTime? TARJETA_VENCE { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string? REGISTRO_USUARIO { get; set; }
    }

    public class AfPersonaMotivoDto
    {
        public int COD_MOTIVO { get; set; }
        public string? DESCRIPCION { get; set; }
        public bool ASIGNADO { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string? REGISTRO_USUARIO { get; set; }
    }

    public class AfPersonaCanalDto
    {
        public string? CANAL_TIPO { get; set; }
        public string? DESCRIPCION { get; set; }
        public bool ASIGNADO { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string? REGISTRO_USUARIO { get; set; }
    }

    public class AfPersonaPreferenciaDto
    {
        public int COD_PREFERENCIA { get; set; }
        public string? DESCRIPCION { get; set; }
        public bool ASIGNADO { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string? REGISTRO_USUARIO { get; set; }
    }

    public class AfPersonaBienDto
    {
        public int COD_BIEN { get; set; }
        public string? DESCRIPCION { get; set; }
        public bool ASIGNADO { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string? REGISTRO_USUARIO { get; set; }
    }
    
    public class AfPersonaEscolaridadDto
    {
        public int COD_ESCOLARIDAD { get; set; }
        public string? DESCRIPCION { get; set; }
        public bool ASIGNADO { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string? REGISTRO_USUARIO { get; set; }
    }

    public class AfPersonaAddRequestDto
    {
        // -------- Bloque Identificación / Estado ----------
        public int TipoId { get; set; }
        public string Cedula { get; set; } = default!;
        public string? Id_Alterno { get; set; }
        public string Nombre_Completo { get; set; } = default!;
        public string? Apellido_1 { get; set; }
        public string? Apellido_2 { get; set; }
        public string? Nombre { get; set; }
        public string? RazonSocial { get; set; }
        public string Estado { get; set; } = "S";          // Ej: 'S' Asociado
        public string EstadoCivil { get; set; } = default!;
        public string Genero { get; set; } = default!;      // 'M' / 'F' / etc.
        public DateTime fNacimiento { get; set; }
        public DateTime fCedulaVence { get; set; }

        // -------- Ingreso / Laboral ----------
        public int PromotorId { get; set; }
        public string? Boleta { get; set; }
        public DateTime fIngreso { get; set; }
        public string EstadoLaboral { get; set; } = default!;

        // -------- Nacimiento / Nacionalidad ----------
        public string PaisNac { get; set; } = default!;
        public string Nacionalidad { get; set; } = default!;

        // -------- Contacto ----------
        public string? Email_1 { get; set; }
        public string? Email_2 { get; set; }

        // -------- Dirección Principal ----------
        public string Provincia { get; set; } = default!;
        public string Canton { get; set; } = default!;
        public string Distrito { get; set; } = default!;
        public string Direccion { get; set; } = default!;
        public string? AptoPostal { get; set; }
        public string? Notificacion { get; set; }

        // -------- Institución / Org ----------
        public int Institucion { get; set; }
        public string? Departamento { get; set; }
        public string? Seccion { get; set; }
        public string? UP { get; set; }
        public string? UT { get; set; }
        public string? CT { get; set; }
        public int Deductora { get; set; }

        // -------- Perfil ----------
        public int Profesion { get; set; }
        public int Sector { get; set; }
        public short NPagos { get; set; }
        public short NHijos { get; set; }
        public decimal PriDeduc { get; set; }          // dec(7,1) -> decimal
        public DateTime fNombramiento { get; set; }
        public int NivelAcademico { get; set; }

        // -------- Jurídico / Económico ----------
        public string? Sociedad { get; set; }
        public string? Actividad { get; set; }
        public short Propiedades { get; set; }         // smallint
        public string Oficina { get; set; } = default!;

        // -------- Redes ----------
        public string? Facebook { get; set; }
        public string? Twitter { get; set; }
        public string? LinkedIn { get; set; }
        public string? Instagram { get; set; }
        public string? Blog { get; set; }

        // -------- Conyuge ----------
        public string? ConyugeCedula { get; set; }
        public string? ConyugeNombre { get; set; }
        public string? ConyugeTelCel { get; set; }
        public string? ConyugeTelTra { get; set; }
        public string? ConyugeTelTraExt { get; set; }

        // -------- Albacea ----------
        public string? AlbaceaCedula { get; set; }
        public string? AlbaceaNombre { get; set; }
        public string? AlbaceaTelCel { get; set; }
        public string? AlbaceaTelTra { get; set; }
        public string? AlbaceaTelTraExt { get; set; }

        // -------- Salario ----------
        public string? SalarioTipo { get; set; }
        public string? SalarioDivisa { get; set; }
        public DateTime? SalarioFecha { get; set; }
        public decimal SalarioDevengado { get; set; }
        public decimal SalarioRebajos { get; set; }
        public decimal SalarioNeto { get; set; }
        public string SalarioEmbargo { get; set; } = "N"; // char(1) 'S'/'N'

        // -------- Flags / Otros ----------
        public short AdministraAportePatronal { get; set; } // smallint
        public short Sugef { get; set; }                   // smallint
        public bool I_Beneficiario { get; set; }           // bit
        public bool I_TrabajoPropio { get; set; }          // bit
        public string? Tipo_Patron { get; set; }           // varchar(10)
        public string? CargoDesc { get; set; }             // varchar(200)

        // -------- PEP ----------
        public short PEP_Ind { get; set; }                 // smallint
        public DateTime? PEP_Inicio { get; set; }
        public DateTime? PEP_Corte { get; set; }
        public string? PEP_Cargo { get; set; }
        public short TipoCES { get; set; }                 // smallint
        public int? C_Actividad { get; set; }

        // -------- Auditoría / Movimiento ----------
        public string Usuario { get; set; } = default!;
        public char Mov { get; set; } = 'A';               // 'A' Agregar / 'E' Editar

        // -------- Dirección de Trabajo (opcionales, SP los limpia si faltan datos) ----------
        public string? TraProvincia { get; set; }
        public string? TraCanton { get; set; }
        public string? TraDistrito { get; set; }
        public string? TraDireccion { get; set; }          // hasta 1000 chars
    }

    public class AfPersonaAddResultDto
    {
        public string? CEDULA { get; set; }
        public int Pass { get; set; }
        public string? Error_Msj { get; set; }
    }

    public class AfPersonaRelacionAddDto
    {
        public string Cedula { get; set; } = default!;
        public int TipoId { get; set; }
        public string CedulaRelacionada { get; set; } = default!;
        public string Apellido1 { get; set; } = "";
        public string Apellido2 { get; set; } = "";
        public string Nombre { get; set; } = "";
        public int TipoVinculo { get; set; }
        public int Parentesco { get; set; }
        public string Usuario { get; set; } = default!;
        public int IdRelacion { get; set; } = 0;
        public int Activo { get; set; } = 1;
    }

    public class AfPersonaRelacionDelDto
    {
        public int IdRelacion { get; set; }
        public string Usuario { get; set; } = default!;
    }

    public class AfPersonaSalarioAddDto
    {
        public string Cedula { get; set; } = default!;
        public string TipoSalario { get; set; } = default!;
        public string Divisa { get; set; } = default!;
        public DateTime Fecha { get; set; }
        public decimal Devengado { get; set; }
        public decimal Rebajos { get; set; }
        public decimal Neto { get; set; }
        public string? Embargos { get; set; }
        public string Usuario { get; set; } = default!;
    }

    public class AfPersonaIngresoEconomicoAddDto
    {
        public string Cedula { get; set; } = default!;
        public decimal Ingreso { get; set; }
        public string Usuario { get; set; } = default!;
        public int Tipo { get; set; } = 1;
    }

    public class AfPersonaDireccionAddDto
    {
        public string Cedula { get; set; } = default!;
        public string Provincia { get; set; } = default!;
        public string Canton { get; set; } = default!;
        public string Distrito { get; set; } = default!;
        public string Direccion { get; set; } = default!;
        public string? Email_01 { get; set; }
        public string? Email_02 { get; set; }
        public string? Telefono_01 { get; set; }
        public string? Telefono_02 { get; set; }
        public string Usuario { get; set; } = default!;
        public string Estado { get; set; } = "A";     // 'A' Activo (como en VB6)
        public string Cod_App { get; set; } = "ProGrX";
        public int Tipo { get; set; } = 2;            // 1=Principal, 2=Trabajo (según tu VB6)
    }

    public class AfPersonaEscolaridadRegistraDto
    {
        public string Cedula { get; set; } = default!;
        public string? CodEscolaridad { get; set; }
        public bool Asignado { get; set; }
        public string Usuario { get; set; } = default!;
    }

    public class AfPersonaPreferenciaRegistraDto
    {
        public string Cedula { get; set; } = default!;
        public int CodPreferencia { get; set; }
        public bool Asignado { get; set; }
        public string Usuario { get; set; } = default!;
    }

    public class AfPersonaCanalRegistraDto
    {
        public string Cedula { get; set; } = default!;
        public string CanalTipo { get; set; } = default!;
        public bool Asignado { get; set; }
        public string Usuario { get; set; } = default!;
    }

    public class AfPersonaPatrimonioVinculaDto
    {
        public string Cedula { get; set; } = default!;
    }

    public class AfPersonaBienesRegistraDto
    {
        public string Cedula { get; set; } = default!;
        public string? CodBien { get; set; }
        public bool Asignado { get; set; }
        public string Usuario { get; set; } = default!;
    }

    public class AfPersonaProductosRegistraDto
    {
        public string cedula { get; set; } = default!;
        public int codproducto { get; set; }
        public bool asignado { get; set; }
        public string usuario { get; set; } = default!;
    }

    public class AfRegistroDefaultDto
    {
        public string Cedula { get; set; } = default!;
        public string Usuario { get; set; } = default!;
    }
    
    public class AfCumplimientoDto
    {
        public int Codigo { get; set; }
        public string? Descripcion { get; set; }
        public bool Asignado { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
    }

    public class AfTelefonosDto
    {
        public string? Telefono { get; set; }
        public string? Tipo { get; set; }
        public string? Numero { get; set; }
        public string? Ext { get; set; }
        public string? Contacto { get; set; }
        public string? Usuario { get; set; }
        public DateTime? Fecha { get; set; }
        public string? TipoDesc { get; set; }
        public DateTime FechaServidor { get; set; }
    }

    public class AfCuentaBancariaDto
    {
        public string? Banco { get; set; }
        public string? TipoDesc { get; set; }
        public string? Cod_Divisa { get; set; }
        public string? Cuenta_Interna { get; set; }
        public string? Cuenta_Interbanca { get; set; }
        public bool Activa { get; set; }
        public string? Destino { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
    }

    public class AfTarjetaDto
    {
        public string? Cedula { get; set; }
        public string? Tarjeta_Numero { get; set; }
        public string? Tarjeta_Tipo { get; set; }
        public string? Tarjeta_Mask { get; set; }
        public string? Tarjeta_Code { get; set; }
        public DateTime? Tarjeta_Vence { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
    }

    public class AfDireccionDto
    {
        public string? Cedula { get; set; }
        public int Linea_Id { get; set; }
        public int Provincia { get; set; }
        public string? ProvinciaDesc { get; set; }
        public int Canton { get; set; }
        public string? CantonDesc { get; set; }
        public int Distrito { get; set; }
        public string? DistritoDesc { get; set; }
        public string? Direccion { get; set; }
        public string? Email_01 { get; set; }
        public string? Email_02 { get; set; }
        public string? Telefono_01 { get; set; }
        public string? Telefono_02 { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
        public string? Cod_App { get; set; }
        public bool? Sincronizada { get; set; }
        public string? Aprobada_Usuario { get; set; }
        public DateTime? Aprobada_Fecha { get; set; }
        public string? Aprobada_Estado { get; set; }
        public int Tipo { get; set; }
        public string? TipoDesc { get; set; }
    }

    public class AfConsultasGeneralesDto
    {
        public List<AfTelefonosDto>? Telefonos { get; set; }
        public List<AfCuentaBancariaDto>? CuentasBancarias { get; set; }
        public List<AfPersonaBeneficiarioDto>? Beneficiarios { get; set; }
        public List<AfTarjetaDto>? Tarjetas { get; set; }
        public List<AfDireccionDto>? Localizaciones { get; set; }
        public List<AfPersonaIngresoDto> Ingresos { get; set; } = new();
        public List<AfPersonaRenunciaDto> Renuncias { get; set; } = new();
        public List<AfPersonaLiquidacionDto> Liquidaciones { get; set; } = new();
        public List<AfPersonaNombramientoDto> Nombramientos { get; set; } = new();
        public List<AfPersonaSalarioDto> Salarios { get; set; } = new();
        public List<AfPersonaEmailDto> Emails { get; set; } = new();
        public List<AfMotivosDto> Motivos { get; set; } = new();
        public List<AfCanalesDto> Canales { get; set; } = new();
        public List<AfPreferenciaDto> Preferencias { get; set; } = new();
        public List<AfBienDto> Bienes { get; set; } = new();
        public List<AfEscolaridadDto> Escolaridad { get; set; } = new();
        public List<AfPersonaRelacionDto> Relaciones { get; set; } = new();
    }

    public class AfMotivosDto
    {
        public string cedula { get; set; } = string.Empty;
        public string? cod_motivo { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class AfCanalesDto
    {
        public string cedula { get; set; } = string.Empty;
        public int canal_tipo { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
    }

    public class AfPreferenciaDto
    {
        public string Cedula { get; set; } = default!;
        public string? cod_preferencia { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class AfBienDto
    {
        public decimal bien_tipo { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
    }

    public class AfEscolaridadDto
    {
        public decimal escolaridad_tipo { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
    }

    public class AfPersonaIndicadoresDto
    {
        public string cedula { get; set; } = default!;
        public int indicador { get; set; }
        public bool valor { get; set; }
        public string usuario { get; set; } = default!;
        public string nota { get; set; } = string.Empty;
    }

    public class AfPadronPersonaDto
    {
        public string identificacion { get; set; } = default!;
        public string apellido_1 { get; set; } = default!;
        public string apellido_2 { get; set; } = default!;
        public string nombre { get; set; } = default!;
        public string sexo { get; set; } = default!;
        public string estado_civil { get; set; } = default!;
        public DateTime fecha_nacimiento { get; set; }
        public string cod_pais { get; set; } = default!;
        public int cod_provincia { get; set; }
        public int cod_canton { get; set; }
        public int cod_distrito { get; set; }
        public string direccion { get; set; } = default!;
        public string? email_01 { get; set; }
        public string? email_02 { get; set; }
        public string? email_03 { get; set; }
        public string? profesion { get; set; }
        public string pais { get; set; } = default!;
        public string provincia { get; set; } = default!;
        public string canton { get; set; } = default!;
        public string distrito { get; set; } = default!;
        public decimal salario { get; set; }
    }

    public class AfPersonaRelacionDtoAdd
    {
        public int pr_id { get; set; }
        public string? cedulasocio { get; set; }
        public string? cedula { get; set; }
        public string? apellido1 { get; set; }
        public string? apellido2 { get; set; }
        public string? nombre { get; set; }
        public int cod_tipo_id { get; set; }
        public int cod_tipo_vinculo { get; set; }
        public string? teltra { get; set; }
        public string? teltraext { get; set; }
        public string? telcell { get; set; }
        public bool activo { get; set; }
        public DateTime registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public int empleado { get; set; }
    }

    public class AfPersonaDimexAddDto
    {
        public string? cedula { get; set; }
        public string? dimex { get; set; }
        public bool activo { get; set; }
        public string? usuario { get; set; }
    }
}