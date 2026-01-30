using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCRPolizasAseguradorasDb
    {
        private readonly PortalDB _portalDB;

        public FrmCRPolizasAseguradorasDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Consulta aseguradoras
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<PolizaAseguradoraDto?> Consultar(int codEmpresa, string codigo)
        {
            var response = new ErrorDto<PolizaAseguradoraDto?>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.QueryFirstOrDefault<PolizaAseguradoraDto>(
                    @"SELECT 
                P.cod_aseguradora,
                P.nombre,
                P.cedula_juridica,
                P.telefono_01,
                P.telefono_02,
                P.tel_fax,
                P.sitio_web,
                P.email_01,
                P.email_02,
                P.apto_postal,
                P.direccion,
                P.provincia,
                P.canton,
                P.distrito,
                P.nombre_contacto,
                P.activo,
                P.codigo_retencion,
                P.formato_tramas,
                P.cod_cuenta,
                P.cod_cuenta_comision,
                Prov.descripcion AS proveedor_desc
            FROM CRD_POLIZAS_ASEGURADORAS P
            LEFT JOIN CxP_Proveedores Prov ON P.cod_proveedor = Prov.cod_proveedor
            WHERE P.cod_aseguradora = @codigo",
                    new { codigo }
                );
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Inserta nueva aseguradora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="m"></param>
        /// <returns></returns>

        public ErrorDto<int> Insertar(int codEmpresa, PolizaAseguradoraDto m)
        {
            var response = new ErrorDto<int> { Code = 0 };

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
        INSERT INTO CRD_POLIZAS_ASEGURADORAS
        (
            cod_aseguradora, nombre, cedula_juridica,
            telefono_01, telefono_02, tel_fax,
            sitio_web, email_01, email_02, apto_postal,
            direccion, provincia, canton, distrito,
            nombre_contacto, activo,
            codigo_retencion, formato_tramas,
            cod_cuenta, cod_cuenta_comision,
            cod_proveedor, registro_fecha, registro_usuario
        )
        VALUES
        (
            @cod_aseguradora, @nombre, @cedula_juridica,
            @telefono_01, @telefono_02, @tel_fax,
            @sitio_web, @email_01, @email_02, @apto_postal,
            @direccion, @provincia, @canton, @distrito,
            @nombre_contacto, @activo,
            @codigo_retencion, @formato_tramas,
            @cod_cuenta, @cod_cuenta_comision,
            @cod_proveedor, GETDATE(), @usuario
        )";

                cn.Execute(sql, new
                {
                    m.cod_aseguradora,
                    m.nombre,
                    m.cedula_juridica,
                    m.telefono_01,
                    m.telefono_02,
                    m.tel_fax,
                    m.sitio_web,
                    m.email_01,
                    m.email_02,
                    m.apto_postal,
                    m.direccion,
                    m.provincia,
                    m.canton,
                    m.distrito,
                    m.nombre_contacto,
                    m.activo,
                    m.codigo_retencion,
                    m.formato_tramas,
                    m.cod_cuenta,
                    m.cod_cuenta_comision,
                    m.cod_proveedor,
                    usuario = "API"
                });

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
        /// Actualiza la aseguradora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public ErrorDto<int> Actualizar(int codEmpresa, PolizaAseguradoraDto m)
        {
            var response = new ErrorDto<int> { Code = 0 };

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
        UPDATE CRD_POLIZAS_ASEGURADORAS SET
            nombre = @nombre,
            cedula_juridica = @cedula_juridica,
            telefono_01 = @telefono_01,
            telefono_02 = @telefono_02,
            tel_fax = @tel_fax,
            sitio_web = @sitio_web,
            email_01 = @email_01,
            email_02 = @email_02,
            apto_postal = @apto_postal,
            direccion = @direccion,
            provincia = @provincia,
            canton = @canton,
            distrito = @distrito,
            nombre_contacto = @nombre_contacto,
            activo = @activo,
            codigo_retencion = @codigo_retencion,
            formato_tramas = @formato_tramas,
            cod_cuenta = @cod_cuenta,
            cod_cuenta_comision = @cod_cuenta_comision,
            cod_proveedor = @cod_proveedor
        WHERE cod_aseguradora = @cod_aseguradora";

                cn.Execute(sql, m);
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
        /// Borra la aseguradora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<int> Borrar(int codEmpresa, string codigo)
        {
            var response = new ErrorDto<int> { Code = 0 };

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Execute(
                    "DELETE FROM CRD_POLIZAS_ASEGURADORAS WHERE cod_aseguradora = @codigo",
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
        /// Metodo de scroll
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigoActual"></param>
        /// <param name="direccion"></param>
        /// <returns></returns>
        public ErrorDto<string?> Scroll(int codEmpresa,string? codigoActual,int direccion)
        {
            var response = new ErrorDto<string?>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                string? codigo = codigoActual;

                // ?? normalización (CLAVE)
                if (string.IsNullOrWhiteSpace(codigo) ||
                    codigo.Equals("undefined", StringComparison.OrdinalIgnoreCase))
                {
                    codigo = null;
                }

                string sql;

                if (codigo == null)
                {
                    sql = direccion > 0
                        ? @"SELECT TOP 1 cod_aseguradora
                    FROM CRD_POLIZAS_ASEGURADORAS
                    ORDER BY cod_aseguradora ASC"
                        : @"SELECT TOP 1 cod_aseguradora
                    FROM CRD_POLIZAS_ASEGURADORAS
                    ORDER BY cod_aseguradora DESC";
                }
                else
                {
                    sql = direccion > 0
                        ? @"SELECT TOP 1 cod_aseguradora
                    FROM CRD_POLIZAS_ASEGURADORAS
                    WHERE cod_aseguradora > @codigo
                    ORDER BY cod_aseguradora ASC"
                        : @"SELECT TOP 1 cod_aseguradora
                    FROM CRD_POLIZAS_ASEGURADORAS
                    WHERE cod_aseguradora < @codigo
                    ORDER BY cod_aseguradora DESC";
                }

                response.Result = cn.QueryFirstOrDefault<string>(
                    sql,
                    new { codigo }
                );
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene las cuentas bancarias
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CuentaBancariaDto>> CuentasBancarias(int codEmpresa,string cedula)
        {
            var response = new ErrorDto<List<CuentaBancariaDto>>();

            try
            {
                using var cn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<CuentaBancariaDto>(
                    @"SELECT 
                C.CUENTA_INTERNA AS cuenta,
                RTRIM(B.Descripcion) AS banco,
                CASE WHEN C.tipo = 'A' THEN 'Ahorros' ELSE 'Corriente' END AS tipo,
                C.cod_divisa AS divisa,
                CASE WHEN C.cuenta_interbanca = 1 THEN 'Sí' ELSE 'No' END AS interbanca,
                C.destino,
                CASE WHEN C.activa = 1 THEN 'Activa' ELSE 'Cerrada' END AS estado,
                C.registro_fecha,
                C.registro_usuario
              FROM SYS_CUENTAS_BANCARIAS C
              INNER JOIN TES_BANCOS_GRUPOS B ON C.cod_banco = B.cod_grupo
              WHERE C.identificacion = @cedula AND C.modulo = 'Pol'",
                    new { cedula }
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
        /// Obtiene las provincias
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ProvinciaaseguradoraDto>> ObtenerProvincias(int codEmpresa)
        {
            var response = new ErrorDto<List<ProvinciaaseguradoraDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<ProvinciaaseguradoraDto>(
                    @"SELECT Provincia, RTRIM(Descripcion) Descripcion
              FROM Provincias
              ").ToList();
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
        public ErrorDto<List<CantonaseguradoraDto>> ObtenerCantones(int codEmpresa,string provincia)
        {
            var response = new ErrorDto<List<CantonaseguradoraDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<CantonaseguradoraDto>(
                    @"SELECT Canton, RTRIM(Descripcion) Descripcion
              FROM Cantones
              WHERE Provincia = @provincia
              ",
                    new { provincia }).ToList();
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
        public ErrorDto<List<DistritoaseguradoraDto>> ObtenerDistritos(int codEmpresa,string provincia,string canton)
        {
            var response = new ErrorDto<List<DistritoaseguradoraDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<DistritoaseguradoraDto>(
                    @"SELECT Provincia AS provincia,
                             Canton AS canton,
                             Distrito AS distrito,
                             RTRIM(Descripcion) AS descripcion
                      FROM Distritos
                      WHERE Provincia = @provincia
                        AND Canton = @canton
                      ",
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
        /// Lista las aseguradoras
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Listar(int codEmpresa)
        {
            string connString = _portalDB.ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                const string sql = @"
                SELECT
                    cod_aseguradora AS item,
                    nombre          AS descripcion
                FROM crd_polizas_aseguradoras
                ORDER BY nombre
            ";

                response.Result = cn
                    .Query<DropDownListaGenericaModel>(sql)
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<DropDownListaGenericaModel>();
            }

            return response;
        }

        /// <summary>
        /// Obtiene los bancos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ObtenerBancos(int codEmpresa, string usuario)
        {
            string connString = _portalDB.ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                var data = cn.Query(
                        "spCrd_SGT_Bancos",
                        new { Usuario = usuario },
                        commandType: CommandType.StoredProcedure
                    );

                                    response.Result = data.Select(x => new DropDownListaGenericaModel
                                    {
                                        item = x.IDX,
                                        descripcion = x.ITMX
                                    }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<DropDownListaGenericaModel>();
            }

            return response;
        }


    }

}




