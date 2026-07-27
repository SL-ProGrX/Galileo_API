using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmArfUnidadesDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;
        private readonly int vModulo = 20;

        public FrmArfUnidadesDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config)) { }

        public FrmArfUnidadesDb(PortalDB portalDb, MSecurityMainDb mProGrxMain)
        {
            _portalDb = portalDb;
            _mSecurityMainDb = mProGrxMain;
        }

        /// <summary>
        /// Obtiene la lista de provincias 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Provincias_Obtener(int codEmpresa)
        {
            string query = @"select Provincia as item, rtrim(Descripcion) as descripcion from Provincias";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de cantones segun la provincia
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codProvincia"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Cantones_Obtener(int codEmpresa, string codProvincia)
        {
            string query = @"select Canton as item, rtrim(Descripcion) as descripcion from Cantones 
                where provincia = @codProvincia order by descripcion";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codProvincia });
        }

        /// <summary>
        /// Obtiene la lista de distritos segun la provincia y canton
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codProvincia"></param>
        /// <param name="codCanton"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Distritos_Obtener(int codEmpresa, string codProvincia, string codCanton)
        {
            string query = @"select Distrito as item, rtrim(Descripcion) as descripcion from Distritos 
                where provincia = @codProvincia and Canton = @codCanton order by descripcion";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codProvincia, codCanton });
        }

        /// <summary>
        /// Obtiene la lista de unidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Unidades_Obtener(int codEmpresa)
        {
            string query = @"select COD_LOCAL as item, Descripcion from ARF_UNIDADES";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene una unidad haciendo scroll, 1 para siguiente, 0 para anterior
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="codUnidad"></param>
        /// <returns></returns>
        public ErrorDto<ArfUnidadesData> ArfUnidades_Scroll_Obtener(int codEmpresa, int scrollCode, string? codUnidad)
        {
            string query = "select Top 1 COD_LOCAL from ARF_UNIDADES ";

            if (!string.IsNullOrEmpty(codUnidad))
            {
                if (scrollCode == 1)
                {
                    query += " where COD_LOCAL > @codUnidad order by COD_LOCAL asc";
                }
                else
                {
                    query += " where COD_LOCAL < @codUnidad order by COD_LOCAL desc";
                }
            }
            
            var codResult = DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, query, null, new { codUnidad });

            if (string.IsNullOrWhiteSpace(codResult?.Result))
            {
                return new ErrorDto<ArfUnidadesData>
                {
                    Code = -2,
                    Description = "No se encontraron registros",
                    Result = null
                };
            }

            return ArfUnidades_ConsultaUnidad_Obtener(codEmpresa, codResult.Result);
        }

        /// <summary>
        /// Obtiene una unidad por su codigo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codUnidad"></param>
        /// <returns></returns>
        public ErrorDto<ArfUnidadesData> ArfUnidades_ConsultaUnidad_Obtener(int codEmpresa, string codUnidad)
        {
            string query = @"select P.*,rtrim(Prov.Descripcion) as ProvDesc, rtrim(Cant.Descripcion) as CantonDesc, rtrim(Dist.Descripcion) as DistDesc
                from ARF_UNIDADES P 
                left join Provincias Prov on P.Provincia = Prov.Provincia
                left join Cantones Cant on P.Provincia = Cant.Provincia and P.Canton = Cant.Canton
                left join Distritos Dist on P.Provincia = Dist.Provincia and P.Canton = Dist.Canton and P.distrito = Dist.distrito
                where P.COD_LOCAL = @codUnidad";
            var result = DbHelper.ExecuteSingleQuery(_portalDb, codEmpresa, query, new ArfUnidadesData(), new { codUnidad });
            result.Result ??= null;
            return result;
        }

        /// <summary>
        /// Guarda o actualiza una unidad
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="existe"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto ArfUnidades_Guardar(int codEmpresa, bool existe, ArfUnidadesData request)
        {
            string usuario = request.registro_usuario;
            if (!existe) // Insertar
            {
                const string sqlInsert = @"
                    insert into ARF_UNIDADES 
                    (COD_LOCAL, Descripcion, Telefono_01, Telefono_02, Activo, cod_Unidad, cod_Centro_Costo, Contacto_Nombre,
                     apto_postal, email_01, email_02, WebSite, provincia, canton, distrito, direccion, Registro_fecha, Registro_usuario)
                    values
                    (@CodLocal, @Descripcion, @Telefono01, @Telefono02, @Activo, @CodUnidad, @CodCentroCosto, @ContactoNombre,
                     @AptoPostal, @Email01, @Email02, @WebSite, @Provincia, @Canton, @Distrito, @Direccion, GETDATE(), @RegistroUsuario);";

                var respInsert = DbHelper.ExecuteNonQuery(
                    _portalDb, codEmpresa, sqlInsert,
                    new
                    {
                        CodLocal = (request.cod_local ?? string.Empty).ToUpperInvariant(),
                        Descripcion = (request.descripcion ?? string.Empty).Trim().ToUpperInvariant(),
                        Telefono01 = (request.telefono_01 ?? string.Empty).Trim(),
                        Telefono02 = (request.telefono_02 ?? string.Empty).Trim(),
                        Activo = request.activo ? 1 : 0,
                        CodUnidad = (request.cod_unidad ?? string.Empty).Trim(),
                        CodCentroCosto = (request.cod_centro_costo ?? string.Empty).Trim(),
                        ContactoNombre = (request.contacto_nombre ?? string.Empty).Trim(),
                        AptoPostal = (request.apto_postal ?? string.Empty).Trim(),
                        Email01 = (request.email_01 ?? string.Empty).Trim(),
                        Email02 = (request.email_02 ?? string.Empty).Trim(),
                        WebSite = (request.website ?? string.Empty).Trim(),
                        Provincia = (request.provincia ?? string.Empty).Trim(),
                        Canton = (request.canton ?? string.Empty).Trim(),
                        Distrito = (request.distrito ?? string.Empty).Trim(),
                        Direccion = (request.direccion ?? string.Empty).Trim(),
                        RegistroUsuario = usuario
                    }
                );

                if (respInsert != null && respInsert.Code < 0) { return respInsert; }

                _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Arrendamiento, Unidad Id: {request.cod_local}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });
            }
            else // Actualizar
            {
                const string sqlUpdate = @"
                    update ARF_UNIDADES set 
                        Descripcion      = @Descripcion,
                        Telefono_01      = @Telefono01,
                        Telefono_02      = @Telefono02,
                        WebSite          = @WebSite,
                        apto_postal      = @AptoPostal,
                        Email_01         = @Email01,
                        email_02         = @Email02,
                        direccion        = @Direccion,
                        Distrito         = @Distrito,
                        canton           = @Canton,
                        Provincia        = @Provincia,
                        Contacto_Nombre  = @ContactoNombre,
                        Activo           = @Activo,
                        cod_Unidad       = @CodUnidad,
                        cod_Centro_Costo = @CodCentroCosto,
                        Modifica_Fecha   = GETDATE(),
                        Modifica_Usuario = @ModificaUsuario
                    where COD_LOCAL = @CodLocal;";

                var respUpdate = DbHelper.ExecuteNonQuery(
                    _portalDb, codEmpresa, sqlUpdate,
                    new
                    {
                        CodLocal = request.cod_local,
                        Descripcion = (request.descripcion ?? string.Empty).Trim().ToUpperInvariant(),
                        Telefono01 = (request.telefono_01 ?? string.Empty).Trim(),
                        Telefono02 = (request.telefono_02 ?? string.Empty).Trim(),
                        WebSite = (request.website ?? string.Empty).Trim(),
                        AptoPostal = (request.apto_postal ?? string.Empty).Trim(),
                        Email01 = (request.email_01 ?? string.Empty).Trim(),
                        Email02 = (request.email_02 ?? string.Empty).Trim(),
                        Direccion = (request.direccion ?? string.Empty).Trim(),
                        Distrito = (request.distrito ?? string.Empty).Trim(),
                        Canton = (request.canton ?? string.Empty).Trim(),
                        Provincia = (request.provincia ?? string.Empty).Trim(),
                        ContactoNombre = (request.contacto_nombre ?? string.Empty).Trim(),
                        Activo = request.activo ? 1 : 0,
                        CodUnidad = (request.cod_unidad ?? string.Empty).Trim(),
                        CodCentroCosto = (request.cod_centro_costo ?? string.Empty).Trim(),
                        ModificaUsuario = usuario
                    }
                );

                if (respUpdate != null && respUpdate.Code < 0) { return respUpdate; }

                _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Arrendamiento, Unidad Id: {request.cod_local}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
            }
            return new ErrorDto { Code = 0, Description = "Informacion guardada satisfactoriamente..." };
        }

        /// <summary>
        /// Elimina una unidad
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codUnidad"></param>
        /// <returns></returns>
        public ErrorDto ArfUnidades_Eliminar(int codEmpresa, string usuario, string codUnidad)
        {
            const string sqlDelete = @"delete ARF_UNIDADES where COD_LOCAL = @CodLocal;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodLocal = codUnidad
                }
            );

            if (respDelete != null && respDelete.Code < 0)
            {
                return respDelete;
            }

            _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Arrendamiento, Unidad Id: {codUnidad}",
                Movimiento = "Elimina - WEB",
                Modulo = vModulo
            });

            return new ErrorDto
            {
                Code = 0,
                Description = "Registro eliminado satisfactoriamente..."
            };
        }

    }
}
