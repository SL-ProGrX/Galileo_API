using System;

namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    // Común para información de dirección
    public class DireccionInfo
    {
        public string? Provincia { get; set; }
        public string? Canton { get; set; }
        public string? Distrito { get; set; }
        public string? Direccion { get; set; }
        public string? Apto_Postal { get; set; }
    }

    // Común para información de contacto
    public class ContactoInfo
    {
        public string? Telefono1 { get; set; }
        public string? Telefono2 { get; set; }
        public string? Celular { get; set; }
        public string? Fax { get; set; }
        public string? Email_01 { get; set; }
        public string? Email_02 { get; set; }
        public string? Website { get; set; }
    }

    // Común para auditoría
    public class AuditoriaInfo
    {
        public string? Registro_Usuario { get; set; }
        public DateTime? Registro_Fecha { get; set; }
    }

    public class CxcPersonaValidaResult
    {
        public int Existe { get; set; }
    }

    public class SocioInfoDto
    {
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public DireccionInfo? DireccionData { get; set; }
        public ContactoInfo? ContactoData { get; set; }
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
        public string? Ultimo_Estado { get; set; }
        public short? Ind_Liquidacion { get; set; }
        public int? Cod_Banco { get; set; }
        public string? Cuenta_Ahorros { get; set; }
        public string? Cod_Departamento { get; set; }
        public int? Cod_Institucion { get; set; }
        public string? Cod_Seccion { get; set; }
        public DateTime? ActualizaFecha { get; set; }
        public string? ActualizaUser { get; set; }
        public short? Bloqueo { get; set; }
        public int? Id_Promotor { get; set; }
        public short? Cod_Profesion { get; set; }
        public short? Cod_Sector { get; set; }
        public int? Boleta { get; set; }
        public string? Cedular { get; set; }
        public short? Af_NPagos { get; set; }
        public string? EstadoActa { get; set; }
        public short? NActa { get; set; }
        public DateTime? FecActa { get; set; }
        public string? Congelar { get; set; }
        public int? PriDeduc { get; set; }
        public string? Pin { get; set; }
        public short? Ind_SinAporte { get; set; }
        public string? Reg_User { get; set; }
        public DateTime? Reg_Fecha { get; set; }
        public int? Id_Boleta_AF { get; set; }
        public string? Nota_User { get; set; }
        public DateTime? Nota_Fecha { get; set; }
        public DateTime? Fecha_Comision { get; set; }
        public short? Ind_Doble_Deduccion { get; set; }
        public short? Tipo_Id { get; set; }
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
        public short? Profesion { get; set; }
        public short? Comision_Autorizada { get; set; }
        public string? Razon_Social { get; set; }
        public string? Cod_Actividad { get; set; }
        public string? Cod_Sociedad { get; set; }
        public short? Ind_Propiedades { get; set; }
        public string? Autoriza_Comision_Notas { get; set; }
        public DateTime? Ben_Update_Fecha { get; set; }
        public string? Ben_Update_Usuario { get; set; }
        public string? Credito_Cls_Tipo { get; set; }
        public DateTime? Credito_Cls_Fecha { get; set; }
        public string? Consentimiento_Contacto_Usuario { get; set; }
        public DateTime? Consentimiento_Contacto_Fecha { get; set; }
        public short? Cliente_VIP { get; set; }
        public string? Email_02 { get; set; }
        public string? Facebook { get; set; }
        public string? Linkedin { get; set; }
        public string? Instagram { get; set; }
        public string? Blog { get; set; }
        public string? Twitter { get; set; }
        public string? Dimex_Cedula { get; set; }
        public string? Crd_Categoria { get; set; }
        public DateTime? Crd_Categoria_Fecha { get; set; }
        public string? Empleado_ID { get; set; }
        public string? Cod_Nacionalidad { get; set; }
        public int? Cod_Deductora { get; set; }
        public string? Dimex_Usuario { get; set; }
        public DateTime? Dimex_Fecha { get; set; }
        public short? Dimex_Activo { get; set; }
        public string? Salario_Usuario { get; set; }
        public DateTime? Salario_Fecha { get; set; }
        public decimal? Salario_Monto { get; set; }
        public string? Salario_Divisa { get; set; }
        public string? Tramite_Resolucion_Usuario { get; set; }
        public DateTime? Tramite_Resolucion_Fecha { get; set; }
        public string? Tramite_Resolucion_Nota { get; set; }
        public DateTime? Dimex_Actualiza_Fecha { get; set; }
        public string? Dimex_Actualiza_Usuario { get; set; }
        public short? Sorteo_Acciones { get; set; }
        public short? Sorteo_Acciones_Prov { get; set; }
        public string? UP { get; set; }
        public string? UT { get; set; }
        public string? CT { get; set; }
        public bool? I_Beneficiarios { get; set; }
        public bool? I_Trabajo_Propio { get; set; }
        public byte? Tipo_Patron { get; set; }
        public string? Cod_Cargo { get; set; }
        public bool? Pep_Ind { get; set; }
        public DateTime? Pep_Inicio { get; set; }
        public DateTime? Pep_Corte { get; set; }
        public string? Pep_Cargo { get; set; }
        public short? Tipo_Ces { get; set; }
        public string? Cod_Pais_Nac { get; set; }
        public short? Ind_Activo { get; set; }
        public DateTime? Fecha_Inactiva { get; set; }
        public DateTime? Fecha_Activa { get; set; }
        public string? EmailSecundario { get; set; }
        public string? NombreV2 { get; set; }
        public string? Apellido1 { get; set; }
        public string? Apellido2 { get; set; }
        public DateTime? Fecha_Ven_Ced { get; set; }
        public string? Nivel_Academico { get; set; }
        public bool? Inscrito_Sugef { get; set; }
        public int? Productos { get; set; }
        public int? Actividades { get; set; }
        public string? Rango { get; set; }
        public string? Direccion_Adicional { get; set; }
        public DateTime? Fecha_Ult_Carga_Act_Datos { get; set; }
        public string? Usuario_Carga_Act_Datos { get; set; }
        public byte? Proyeccion_Social { get; set; }
        public bool? AutorizaAdminAportePatronal { get; set; }
        public string? Albacea_TelTra { get; set; }
        public string? Albacea_TelTraExt { get; set; }
        public string? Albacea_TelCell { get; set; }
        public int? Id_Persona { get; set; }
        public string? Tra_Provincia { get; set; }
        public string? Tra_Canton { get; set; }
        public string? Tra_Distrito { get; set; }
        public string? Tra_Direccion { get; set; }
        // Campos extendidos del query
        public string? ProvDesc { get; set; }
        public string? CantonDesc { get; set; }
        public string? DistDesc { get; set; }
        public string? TipoIdDesc { get; set; }
        public string? Tipo_Personeria { get; set; }
        public string? TelHab { get; set; }
        public string? TelTra { get; set; }
        public string? TelCell { get; set; }
    }

    public class PersonaInfoDto
    {
        public string? Cedula { get; set; }
        public short Tipo_Id { get; set; }
        public string? Nombre { get; set; }
        public string? Razon_Social { get; set; }
        public int? Enlace_CXP { get; set; }
        public ContactoInfo? ContactoData { get; set; }
        public DireccionInfo? DireccionData { get; set; }
        public string? Sexo { get; set; }
        public string? EstadoCivil { get; set; }
        public DateTime? Fecha_Nacimiento { get; set; }
        public string? Notas { get; set; }
        public short? Credito_Cerrado { get; set; }
        public short? Cliente_Exento { get; set; }
        public DateTime? Categoria_Fecha { get; set; }
        public string? Cod_Categoria { get; set; }
        public short? Adelanto_Permite { get; set; }
        public decimal? Adelanto_Porcentaje { get; set; }
        public short? Adelanto_Modifica { get; set; }
        public short? Activo { get; set; }
        public decimal? Credito_Limite { get; set; }
        public string? Registro_Usuario { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public short? Adelanto_Comision_Apl { get; set; }
        public decimal? Adelanto_Comision { get; set; }
        public short? Rol_Pagador { get; set; }
        public short? Rol_Autorizador { get; set; }
        // Campos extendidos del query
        public string? ProvDesc { get; set; }
        public string? CantonDesc { get; set; }
        public string? DistDesc { get; set; }
        public string? TipoIdDesc { get; set; }
        public string? Tipo_Personeria { get; set; }
        public string? CatDesc { get; set; }
        public string? EstadoCivilDesc { get; set; }
    }

    public class CxcPersonaLargoCedulaResult
    {
        public int LARGO_MINIMO { get; set; }
    }

    public class CxcPersonaSaveParams
    {
        public string? Cedula { get; set; }
        public short Tipo_Id { get; set; }
        public string? Nombre { get; set; }
        public string? Razon_Social { get; set; }
        public string? Celular { get; set; }
        public string? Telefono1 { get; set; }
        public string? Telefono2 { get; set; }
        public string? Fax { get; set; }
        public string? Sexo { get; set; }
        public string? EstadoCivil { get; set; }
        public DateTime? Fecha_Nacimiento { get; set; }
        public string? Apto_Postal { get; set; }
        public string? Email_01 { get; set; }
        public string? Email_02 { get; set; }
        public string? Website { get; set; }
        public string? Notas { get; set; }
        public string? Direccion { get; set; }
        public string? Distrito { get; set; }
        public string? Canton { get; set; }
        public short? Provincia { get; set; }
        public short Credito_Cerrado { get; set; }
        public short Cliente_Exento { get; set; }
        public string? Cod_Categoria { get; set; }
        public short Adelanto_Permite { get; set; }
        public short Adelanto_Modifica { get; set; }
        public decimal? Adelanto_Porcentaje { get; set; }
        public decimal? Credito_Limite { get; set; }
        public short Activo { get; set; }
        public short Adelanto_Comision_Apl { get; set; }
        public decimal? Adelanto_Comision { get; set; }
        public short Rol_Pagador { get; set; }
        public short Rol_Autorizador { get; set; }
        public string? Usuario { get; set; }
    }

    public class CxcPersonaDeleteParams
    {
        public required string Cedula { get; set; }
        public required string Usuario { get; set; }
    }
}
