using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCClientesDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;

        public FrmCxCClientesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _dbBitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Consulta la lista de personas, ordenadas por el campo indicado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="orden">Campo de ordenamiento ("Cedula" o "Nombre").</param>
        /// <returns>Lista de personas.</returns>
        public ErrorDto<List<CxcPersonaDto>> CxcPersonas_Lista(int codEmpresa, string orden)
        {
            string orderBy = orden?.ToLower() == "cedula" ? "cedula" : "nombre";
            var query = $@"
                SELECT cedula, nombre
                FROM CxC_Personas
                ORDER BY {orderBy}";
            return DbHelper.ExecuteListQuery<CxcPersonaDto>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de estados civiles activos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de estados civiles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> EstadoCivil_Lista(int codEmpresa)
        {
            var query = @"
                SELECT rtrim(Estado_Civil) as item,
                       rtrim(DESCRIPCION) as descripcion
                FROM SYS_ESTADO_CIVIL
                WHERE ACTIVO = 1";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de clasificaciones de clientes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de clasificaciones.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Clasificacion_Lista(int codEmpresa)
        {
            var query = @"
                SELECT rtrim(cod_categoria) as item,
                       rtrim(descripcion) as descripcion
                FROM CxC_Categoria_Clientes";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de tipos de identificación.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de tipos de identificación.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TiposId_Lista(int codEmpresa)
        {
            var query = @"
                SELECT TIPO_ID as item,
                       rtrim(Descripcion) as descripcion
                FROM AFI_TIPOS_IDS";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de provincias.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de provincias.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Provincias_Lista(int codEmpresa)
        {
            var query = @"
                SELECT Provincia as item,
                       rtrim(Descripcion) as descripcion
                FROM Provincias";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de cantones por provincia.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="provincia">Código de la provincia.</param>
        /// <returns>Lista de cantones.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cantones_Lista(int codEmpresa, string provincia)
        {
            var query = @"
                SELECT Canton as item,
                       rtrim(Descripcion) as descripcion
                FROM Cantones
                WHERE provincia = @provincia
                ORDER BY descripcion";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { provincia });
        }

        /// <summary>
        /// Obtiene la lista de distritos por provincia y cantón.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="provincia">Código de la provincia.</param>
        /// <param name="canton">Código del cantón.</param>
        /// <returns>Lista de distritos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Distritos_Lista(int codEmpresa, string provincia, string canton)
        {
            var query = @"
                SELECT Distrito as item,
                       rtrim(Descripcion) as descripcion
                FROM Distritos
                WHERE provincia = @provincia
                  AND canton = @canton
                ORDER BY descripcion";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { provincia, canton });
        }

        /// <summary>
        /// Valida si existe una persona por cédula.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Resultado de validación.</returns>
        public ErrorDto<CxcPersonaValidaResult?> CxcPersona_Valida(int codEmpresa, string cedula)
        {
            var query = @"select isnull(count(*),0) as Existe from cxc_personas where cedula = @cedula";
            return DbHelper.ExecuteSingleQuery<CxcPersonaValidaResult>(_portalDb, codEmpresa, query, default, new { cedula });
        }

        /// <summary>
        /// Obtiene la información extendida de un socio.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <returns>Información del socio.</returns>
        public ErrorDto<SocioInfoDto?> Socio_Info(int codEmpresa, string cedula)
        {
            var query = @"
                select P.*,
                       rtrim(Prov.Descripcion) as ProvDesc,
                       rtrim(Cant.Descripcion) as CantonDesc,
                       rtrim(Dist.Descripcion) as DistDesc,
                       Tid.Descripcion as TipoIdDesc,
                       Tid.Tipo_Personeria,
                       dbo.fxAFITelefono(P.cedula,1) as TelHab,
                       dbo.fxAFITelefono(P.cedula,2) as TelTra,
                       dbo.fxAFITelefono(P.cedula,3) as TelCell
                from socios P
                left join Provincias Prov on P.Provincia = Prov.Provincia
                left join Cantones Cant on P.Provincia = Cant.Provincia and P.Canton = Cant.Canton
                left join Distritos Dist on P.Provincia = Dist.Provincia and P.Canton = Dist.Canton and P.distrito = Dist.distrito
                left join AFI_TIPOS_IDS Tid on P.tipo_id = Tid.tipo_id
                where P.cedula = @cedula";
            return DbHelper.ExecuteSingleQuery<SocioInfoDto>(_portalDb, codEmpresa, query, default, new { cedula });
        }

        /// <summary>
        /// Obtiene la información extendida de una persona.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Información de la persona.</returns>
        public ErrorDto<PersonaInfoDto?> Persona_Info(int codEmpresa, string cedula)
        {
            var query = @"
                select P.*,
                       rtrim(Prov.Descripcion) as ProvDesc,
                       rtrim(Cant.Descripcion) as CantonDesc,
                       rtrim(Dist.Descripcion) as DistDesc,
                       Tid.Descripcion as TipoIdDesc,
                       Tid.Tipo_Personeria,
                       rtrim(Cat.Descripcion) as CatDesc,
                       isnull(Ec.Descripcion,'No. Identificado') as EstadoCivilDesc
                from CxC_Personas P
                left join Provincias Prov
                       on P.Provincia = Prov.Provincia
                left join Cantones Cant
                       on P.Provincia = Cant.Provincia
                      and P.Canton = Cant.Canton
                left join Distritos Dist
                       on P.Provincia = Dist.Provincia
                      and P.Canton = Dist.Canton
                      and P.distrito = Dist.distrito
                left join AFI_TIPOS_IDS Tid
                       on P.tipo_id = Tid.tipo_id
                left join CxC_Categoria_Clientes Cat
                       on P.cod_categoria = Cat.cod_Categoria
                left join SYS_ESTADO_CIVIL Ec
                       on P.EstadoCivil = Ec.Estado_Civil
                where P.cedula = @cedula";
            return DbHelper.ExecuteSingleQuery<PersonaInfoDto>(_portalDb, codEmpresa, query, default, new { cedula });
        }

        /// <summary>
        /// Valida el largo mínimo de la cédula para un tipo de ID.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="tipoId">Tipo de identificación.</param>
        /// <returns>Resultado con el largo mínimo.</returns>
        public ErrorDto<CxcPersonaLargoCedulaResult?> CxcPersona_LargoCedula(int codEmpresa, short tipoId)
        {
            var query = @"select LARGO_MINIMO from AFI_TIPOS_IDS where TIPO_ID = @tipoId";
            return DbHelper.ExecuteSingleQuery<CxcPersonaLargoCedulaResult>(_portalDb, codEmpresa, query, default, new { tipoId });
        }

        /// <summary>
        /// Guarda (inserta o actualiza) una persona.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la persona a guardar.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcPersona_Guardar(int codEmpresa, CxcPersonaSaveParams param)
        {

            if (param.Persona == null || string.IsNullOrWhiteSpace(param.Persona.Cedula))
            {
                return new ErrorDto<bool>
                {
                    Code = -1,
                    Description = "Los datos de la persona o la cédula son requeridos.",
                    Result = false
                };
            }

            // Verifica existencia
            var existe = DbHelper.ExecuteSingleQuery<int>(
                _portalDb, codEmpresa,
                "SELECT COUNT(1) FROM CxC_Personas WHERE cedula = @Cedula",
                default, new { param.Persona.Cedula }
            ).Result;

            var dbParams = MapToDbParams(param);

            if (existe == 0)
            {
                // Insertar
                var sql = @"INSERT INTO CxC_Personas(
                    cedula, Tipo_Id, nombre, razon_social, celular, telefono1, telefono2, fax,
                    sexo, estadoCivil, fecha_nacimiento, apto_postal, email_01, email_02,
                    webSite, notas, direccion, distrito, provincia, canton,
                    credito_cerrado, Cliente_Exento, cod_categoria, categoria_fecha,
                    ADELANTO_PERMITE, ADELANTO_MODIFICA, ADELANTO_PORCENTAJE, CREDITO_LIMITE,
                    ACTIVO, ADELANTO_COMISION_APL, ADELANTO_COMISION, ROL_PAGADOR, ROL_AUTORIZADOR
                )
                VALUES(
                    @Cedula, @Tipo_Id, @Nombre, @Razon_Social, @Celular, @Telefono1, @Telefono2, @Fax,
                    @Sexo, @EstadoCivil, @Fecha_Nacimiento, @Apto_Postal, @Email_01, @Email_02,
                    @Website, @Notas, @Direccion, @Distrito, @Provincia, @Canton,
                    @Credito_Cerrado, @Cliente_Exento, @Cod_Categoria, dbo.MyGetdate(),
                    @Adelanto_Permite, @Adelanto_Modifica, @Adelanto_Porcentaje, @Credito_Limite,
                    @Activo, @Adelanto_Comision_Apl, @Adelanto_Comision, @Rol_Pagador, @Rol_Autorizador
                )";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sql, dbParams);
                    return rows > 0;
                });

                RegistrarBitacora(codEmpresa, param.Usuario ?? "", param.Persona.Cedula ?? "", "REGISTRA-WEB");
                return result;
            }
            else
            {
                // Actualizar
                var sql = @"UPDATE CxC_Personas
                    SET nombre = @Nombre,
                        Razon_Social = @Razon_Social,
                        Tipo_Id = @Tipo_Id,
                        telefono1 = @Telefono1,
                        telefono2 = @Telefono2,
                        celular = @Celular,
                        Fax = @Fax,
                        WebSite = @Website,
                        apto_postal = @Apto_Postal,
                        email_01 = @Email_01,
                        email_02 = @Email_02,
                        direccion = @Direccion,
                        distrito = @Distrito,
                        canton = @Canton,
                        provincia = @Provincia,
                        sexo = @Sexo,
                        EstadoCivil = @EstadoCivil,
                        Fecha_nacimiento = @Fecha_Nacimiento,
                        notas = @Notas,
                        credito_cerrado = @Credito_Cerrado,
                        Cliente_Exento = @Cliente_Exento,
                        cod_categoria = @Cod_Categoria,
                        Categoria_Fecha = dbo.MyGetdate(),
                        ADELANTO_PERMITE = @Adelanto_Permite,
                        ADELANTO_MODIFICA = @Adelanto_Modifica,
                        ADELANTO_PORCENTAJE = @Adelanto_Porcentaje,
                        CREDITO_LIMITE = @Credito_Limite,
                        ACTIVO = @Activo,
                        ADELANTO_COMISION_APL = @Adelanto_Comision_Apl,
                        ADELANTO_COMISION = @Adelanto_Comision,
                        ROL_PAGADOR = @Rol_Pagador,
                        ROL_AUTORIZADOR = @Rol_Autorizador
                    WHERE cedula = @Cedula";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sql, dbParams);
                    return rows > 0;
                });

                RegistrarBitacora(codEmpresa, param.Usuario ?? "", param.Persona.Cedula ?? "", "MODIFICA-WEB");
                return result;
            }
        }

        /// <summary>
        /// Elimina una persona.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la persona a eliminar.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcPersona_Eliminar(int codEmpresa, CxcPersonaDeleteParams param)
        {
            var sql = @"DELETE FROM CxC_Personas WHERE cedula = @Cedula";
            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });

            RegistrarBitacora(codEmpresa, param.Usuario, param.Cedula, "ELIMINA-WEB");
            return result;
        }

        /// <summary>
        /// Obtiene la lista de cuentas de una persona según su estado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <param name="estado">Estado de la cuenta ('A', 'C', 'N', 'T', etc.).</param>
        /// <returns>Lista de cuentas de la persona.</returns>
        public ErrorDto<List<CxcPersonaCuentaDto>> CxcPersonasCuentas(int codEmpresa, string cedula, string estado)
        {
            var sql = "exec spCxC_PersonasCuentas @Cedula, @Estado";
            var param = new { Cedula = cedula, Estado = estado };
            return DbHelper.ExecuteListQuery<CxcPersonaCuentaDto>(_portalDb, codEmpresa, sql, param);
        }

        // Métodos privados no requieren comentarios XML para documentación pública.
        private void RegistrarBitacora(int codEmpresa, string usuario, string cedula, string movimiento)
        {
            _dbBitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario?.ToUpperInvariant() ?? "",
                DetalleMovimiento = $"Persona: {cedula}",
                Movimiento = movimiento,
                Modulo = 9
            });
        }

        private static object MapToDbParams(CxcPersonaSaveParams param)
        {
            var p = param.Persona ?? new PersonaBaseInfo();
            var c = param.ContactoData ?? new ContactoInfo();
            var d = param.DireccionData ?? new DireccionInfo();

            return new
            {
                // PersonaBaseInfo
                p.Cedula,
                p.Tipo_Id,
                p.Nombre,
                p.Razon_Social,
                p.Sexo,
                p.EstadoCivil,
                p.Fecha_Nacimiento,
                p.Credito_Cerrado,
                p.Cliente_Exento,
                p.Cod_Categoria,
                p.Adelanto_Permite,
                p.Adelanto_Porcentaje,
                p.Adelanto_Modifica,
                p.Activo,
                p.Credito_Limite,
                p.Adelanto_Comision_Apl,
                p.Adelanto_Comision,
                p.Rol_Pagador,
                p.Rol_Autorizador,
                p.Notas,

                // ContactoInfo
                c.Telefono1,
                c.Telefono2,
                c.Celular,
                c.Fax,
                c.Email_01,
                c.Email_02,
                c.Website,

                // DireccionInfo
                d.Provincia,
                d.Canton,
                d.Distrito,
                d.Direccion,
                d.Apto_Postal
            };
        }
    }
}
