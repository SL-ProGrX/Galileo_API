using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_ARF
{
    public class FrmArfAcreedoresDb
    {
        private readonly PortalDB _portalDb;

        public FrmArfAcreedoresDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Consulta acreedores
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<ArfAcreedorDto?> Consultar(int codEmpresa, int codigo)
        {
            var response = new ErrorDto<ArfAcreedorDto?>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.QueryFirstOrDefault<ArfAcreedorDto>(
                    @"SELECT  A.*,
                              P.descripcion AS proveedor_desc,
                              ISNULL(Cta.Cod_Cuenta_Mask, A.cod_Cuenta) AS cod_cuenta_mask,
                              ISNULL(Cta.Descripcion, '') AS cuenta_desc
                      FROM ARF_ACREEDORES A
                      LEFT JOIN CxP_Proveedores P
                             ON A.cod_proveedor = P.cod_proveedor
                      LEFT JOIN vCNTX_CUENTAS_LOCAL Cta
                             ON A.cod_Cuenta = Cta.Cod_Cuenta
                      WHERE A.COD_ACREEDOR = @codigo",
                    new { codigo });

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Inserta Acreedores
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public ErrorDto<int> Insertar(int codEmpresa, ArfAcreedorDto m)
        {
            var response = new ErrorDto<int>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Execute(
                    @"INSERT INTO ARF_ACREEDORES
                                    (
                                     cod_acreedor,
                                     descripcion,
                                     tipo_id,
                                     identificacion,
                                     telefono_01,
                                     telefono_02,
                                     activo,
                                     apto_postal,
                                     email_01,
                                     email_02,
                                     website,
                                     provincia,
                                     canton,
                                     distrito,
                                     direccion,
                                     contacto_nombre,
                                     cod_banco,
                                     cod_cuenta,
                                     cod_proveedor,
                                     registro_fecha,
                                     registro_usuario
                                    )
                                    VALUES
                                    (
                                     @cod_acreedor,
                                     @descripcion,
                                     @tipo_id,
                                     @identificacion,
                                     @telefono_01,
                                     @telefono_02,
                                     @activo,
                                     @apto_postal,
                                     @email_01,
                                     @email_02,
                                     @website,
                                     @provincia,
                                     @canton,
                                     @distrito,
                                     @direccion,
                                     @contacto_nombre,
                                     @cod_banco,
                                     @cod_cuenta,
                                     @cod_proveedor,
                                     GETDATE(),
                                     @usuario
                                    )",
                    m);

                response.Result = 1;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Actualiza Acreedores
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public ErrorDto<int> Actualizar(int codEmpresa, ArfAcreedorDto m)
        {
            var response = new ErrorDto<int>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Execute(
                      @"UPDATE ARF_ACREEDORES
                          SET descripcion = @descripcion,
                              tipo_id = @tipo_id,
                              identificacion = @identificacion,
                              telefono_01 = @telefono_01,
                              telefono_02 = @telefono_02,
                              website = @website,
                              apto_postal = @apto_postal,
                              email_01 = @email_01,
                              email_02 = @email_02,
                              direccion = @direccion,
                              provincia = @provincia,
                              canton = @canton,
                              distrito = @distrito,
                              contacto_nombre = @contacto_nombre,
                              activo = @activo,
                              cod_banco = @cod_banco,
                              cod_cuenta = @cod_cuenta,
                              cod_proveedor = @cod_proveedor,
                              modifica_fecha = GETDATE(),
                              modifica_usuario = @usuario
                          WHERE cod_acreedor = @cod_acreedor",
                      m);

                response.Result = 1;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Borra un acreedor
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<int> Borrar(int codEmpresa, int codigo)
        {
            var response = new ErrorDto<int>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Execute(
                    "DELETE FROM ARF_ACREEDORES WHERE COD_ACREEDOR = @codigo",
                    new { codigo });

                response.Result = 1;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Metodo de Scroll
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigoActual"></param>
        /// <param name="direccion"></param>
        /// <returns></returns>
        public ErrorDto<int?> Scroll(int codEmpresa, int? codigoActual, int direccion)
        {
            var response = new ErrorDto<int?>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                string sql = direccion > 0
                    ? @"SELECT TOP 1 COD_ACREEDOR
                        FROM ARF_ACREEDORES
                        WHERE COD_ACREEDOR > @codigoActual
                        ORDER BY COD_ACREEDOR ASC"
                    : @"SELECT TOP 1 COD_ACREEDOR
                        FROM ARF_ACREEDORES
                        WHERE COD_ACREEDOR < @codigoActual
                        ORDER BY COD_ACREEDOR DESC";

                response.Result = cn.QueryFirstOrDefault<int?>(
                    sql,
                    new { codigoActual = codigoActual ?? 0 });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene cuentas bancarias
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <returns></returns>
        public ErrorDto<List<CuentaBancariaAcreedorDto>> CuentasBancarias(int codEmpresa,string identificacion)
        {
            var response = new ErrorDto<List<CuentaBancariaAcreedorDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<CuentaBancariaAcreedorDto>(
                    @"SELECT CUENTA_INTERNA AS cuenta,
                             cod_divisa AS divisa
                      FROM SYS_CUENTAS_BANCARIAS
                      WHERE identificacion = @identificacion
                        AND modulo = 'ARF'",
                    new { identificacion }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    
     /// <summary>
     /// Obtiene las provincias
     /// </summary>
     /// <param name="codEmpresa"></param>
     /// <returns></returns>
    public ErrorDto<List<ProvinciaAcreedorDto>> ObtenerProvincias(int codEmpresa)
        {
            var response = new ErrorDto<List<ProvinciaAcreedorDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<ProvinciaAcreedorDto>(
                    @"SELECT provincia,
                     descripcion
              FROM PROVINCIAS"
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Obtiene los cantones
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="provincia"></param>
        /// <returns></returns>
        public ErrorDto<List<CantonAcreedorDto>> ObtenerCantones(int codEmpresa, string provincia)
        {
            var response = new ErrorDto<List<CantonAcreedorDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<CantonAcreedorDto>(
                    @"SELECT canton,
                     descripcion
              FROM CANTONES
              WHERE provincia = @provincia",
                    new { provincia }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Obtiene los distritos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="provincia"></param>
        /// <param name="canton"></param>
        /// <returns></returns>
        public ErrorDto<List<DistritoAcreedorDto>> ObtenerDistritos(int codEmpresa, string provincia, string canton)
        {
            var response = new ErrorDto<List<DistritoAcreedorDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<DistritoAcreedorDto>(
                    @"SELECT provincia,
                     canton,
                     distrito,
                     descripcion
              FROM DISTRITOS
              WHERE provincia = @provincia
                AND canton = @canton",
                    new { provincia, canton }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Obtiene los tipos de identificacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ObtenerTiposIdentificacion(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<DropDownListaGenericaModel>(
                    @"SELECT tipo_id AS item,
                     descripcion
              FROM AFI_TIPOS_IDS
              ORDER BY descripcion"
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Busca los acreedores
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<ArfAcreedorDto>> BuscarAcreedores(int codEmpresa, string? filtro)
        {
            var response = new ErrorDto<List<ArfAcreedorDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<ArfAcreedorDto>(
                    @"SELECT cod_acreedor,
                     descripcion,
                     identificacion
              FROM ARF_ACREEDORES
              WHERE (@filtro IS NULL
                     OR descripcion LIKE '%' + @filtro + '%'
                     OR identificacion LIKE '%' + @filtro + '%')
              ORDER BY descripcion",
                    new { filtro }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Busca los proveedores
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> BuscarProveedores(int codEmpresa, string? filtro)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<DropDownListaGenericaModel>(
                    @"SELECT cod_proveedor as item, descripcion
              FROM CxP_Proveedores",
                    new { filtro }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene los bancos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ObtenerBancos(int codEmpresa,string usuario)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<DropDownListaGenericaModel>(
                    "exec spCrd_SGT_Bancos @usuario",
                    new { usuario }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }




    }
}