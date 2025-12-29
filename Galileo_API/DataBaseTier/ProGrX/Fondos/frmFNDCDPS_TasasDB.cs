using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Http;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndCdpsTasasDb
    {
        private readonly int vModulo = 18;
        private readonly MSecurityMainDb _mSecurity;
        private readonly PortalDB _portalDB;

        public FrmFndCdpsTasasDb(IConfiguration config)
        {
            _mSecurity = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtener Catálogo de CDP Tasas
        /// 1 - Frecuencias Cupones
        /// 2 - Vencimiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_CdpsTasas_Catalogo_Obtener(int CodEmpresa, int Index)
        {
            string query = Index switch
            {
                // 0 - Frecuencias Cupones
                0 => "select ID_FRECUENCIACUPON AS item, Cupon as descripcion From FND_CDP_FRECUENCIACUPONES Where Estado = 1 order by FRECUENCIA_MESES",

                // 1 - Vencimiento
                1 => "select ID_PLAZO as item, Plazo as descripcion From FND_CDP_PLAZOS Where Estado = 1 Order by PLAZO_MESES",

                // Default → retorna vacío
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(query))
            {
                var response = new ErrorDto<List<DropDownListaGenericaModel>>();
                response.Code = -1;
                response.Description = "Opción inválida.";
                response.Result = null;
                return response;
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Obtener CDP Tasas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Exporta"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> Fnd_CdpsTasas_Obtener(int CodEmpresa, bool Exporta, FiltrosLazyLoadData filtros)
        {

            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel()
                {
                    total = 0,
                    lista = new List<FndCdpsTasaRefData>()
                }
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                string query = "select COUNT(*) from FND_CDPS_TASA_REF";
                response.Result.total = connection.Query<int>(query).FirstOrDefault();

                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    filtros.filtro = " WHERE ( " +
                        " cod_tasa_ref LIKE '%" + filtros.filtro + "%' " +
                        " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                        " OR cod_divisa LIKE '%" + filtros.filtro + "%' ) ";
                }

                if (string.IsNullOrEmpty(filtros.sortField))
                {
                    filtros.sortField = "cod_tasa_ref";
                }

                query = $@"select * from FND_CDPS_TASA_REF
                    {filtros.filtro}
                    order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}";

                if (!Exporta)
                {
                    query += $@" OFFSET {filtros.pagina} ROWS 
                         FETCH NEXT {filtros.paginacion} ROWS ONLY";
                }

                response.Result.lista = connection.Query<FndCdpsTasaRefData>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Guardar Configuración de CDP Tasa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Fnd_CdpsTasas_Config_Guardar(int CodEmpresa, FndCdpsTasaRefData data)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"exec spFnd_CDP_Tasa_Config_Add @Codigo, @Descripcion, @Divisa, @Activo, @Usuario";

                var spResp = connection.QueryFirstOrDefault<SpCdpTasaConfigResultDto>(sql, new
                {
                    Codigo = data.cod_tasa_ref.Trim(),
                    Descripcion = (data.descripcion ?? "").Trim(),
                    Divisa = (data.cod_divisa ?? "").Trim(),
                    Activo = data.activo,
                    Usuario = data.registro_usuario.Trim().ToUpper()
                });

                if (spResp == null)
                {
                    response.Code = -1;
                    response.Description = "El procedimiento no retorn&oacute; respuesta.";
                    return response;
                }

                if (spResp.pass == 1)
                {
                    _mSecurity.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = data.registro_usuario.Trim().ToUpper(),
                        Movimiento = spResp.movimiento + " - WEB",
                        DetalleMovimiento = $"CDPS Modelo de Tasas: {data.cod_tasa_ref.Trim()}",
                        Modulo = vModulo
                    });
                }
                else
                {
                    response.Code = -1;
                    response.Description = spResp.mensaje ?? "Error al guardar la configuraci&oacute;n.";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtener Planes de CDP Tasa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodTasaRef"></param>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<FndCdpsTasaPlanesDto>> Fnd_CdpsTasas_Planes_Obtener(int CodEmpresa, string CodTasaRef, string? Filtro)
        {
            Filtro ??= string.Empty;

            const string query = @"
                select
                    Pl.cod_Operadora,
                    Pl.cod_Plan,
                    Pl.Descripcion,
                    Asg.registro_Fecha,
                    Asg.Registro_Usuario
                from Fnd_Planes Pl
                left join FND_CDPS_TASA_PLANES Asg
                    on Pl.cod_operadora = Asg.cod_Operadora
                    and Pl.cod_Plan = Asg.Cod_Plan
                    and Asg.COD_TASA_REF = @CodTasaRef
                where
                    Pl.Estado = 'A'
                    and Pl.Tipo_CDP = 1
                    and Pl.PAGO_CUPONES = 1
                    and (Pl.Cod_Plan like @Filtro or Pl.Descripcion like @Filtro)
                order by
                    isnull(Asg.Cod_Plan,'ZZZZZZZZZZZZ') asc,
                    Pl.cod_Plan asc;";

            return DbHelper.ExecuteListQuery<FndCdpsTasaPlanesDto>(
                _portalDB,
                CodEmpresa,
                query,
                new
                {
                    CodTasaRef,
                    Filtro = $"%{Filtro}%"
                });
        }

        /// <summary>
        /// Asignar o Desasignar Planes de CDP Tasa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodTasaRef"></param>
        /// <param name="CodPlan"></param>
        /// <param name="Usuario"></param>
        /// <param name="Accion"></param>
        /// <returns></returns>
        public ErrorDto Fnd_CdpsTasas_Plan_Asignar(int CodEmpresa, string CodTasaRef, string CodPlan, string Usuario, int Accion)
        {
            string sql = @"exec spFnd_CDP_Tasa_Plan_Add @CodTasaRef, @CodPlan, @Usuario";

            if (Accion == 0) // Agregar
            {
                sql += ", 'A'";
            }
            else // Eliminar
            {
                sql += ", 'E'";
            }

            return DbHelper.ExecuteNonQuery(
                _portalDB,
                CodEmpresa,
                sql,
                new
                {
                    CodTasaRef = CodTasaRef.Trim(),
                    CodPlan = CodPlan.Trim(),
                    Usuario = Usuario.Trim().ToUpper()
                });
        }

        /// <summary>
        /// Obtener Vencimiento de CDP Tasa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodTasaRef"></param>
        /// <param name="IdPlazo"></param>
        /// <returns></returns>
        public ErrorDto<List<FndCdpTasasVencimientoDto>> Fnd_CdpsTasas_Vencimiento_Obtener(int CodEmpresa, string CodTasaRef, int IdPlazo)
        {
            const string query = @"select T.*, C.CUPON , V.PLAZO 
                    from FND_CDP_TASACUPONES T
                    inner join FND_CDP_FRECUENCIACUPONES C
                        on T.ID_FRECUENCIACUPON = C.ID_FRECUENCIACUPON
                    inner join FND_CDP_PLAZOS V
                        on T.ID_PLAZOCUPON = V.ID_PLAZO
                    where
                        T.COD_TASA_REF = @CodTasaRef
                        and V.ID_PLAZO = @IdPlazo;";

            return DbHelper.ExecuteListQuery<FndCdpTasasVencimientoDto>(
                _portalDB,
                CodEmpresa,
                query,
                new { CodTasaRef, IdPlazo }
                );
        }

        /// <summary>
        /// Guardar Vencimiento de CDP Tasa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodTasaRef"></param>
        /// <param name="IdCupon"></param>
        /// <param name="IdPlazo"></param>
        /// <param name="Tasa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto Fnd_CdpsTasas_Vencimiento_Guardar(int CodEmpresa, string CodTasaRef, int IdCupon, int IdPlazo, decimal Tasa, string Usuario)
        {

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = "exec spFnd_CDP_Tasas_Add @TasaCod, @fCuponId, @PlazoId, @Tasa, @Estado, @Usuario";

                var spResp = connection.QueryFirstOrDefault<SpCdpTasaConfigResultDto>(sql, new
                {
                    TasaCod = CodTasaRef,
                    fCuponId = IdCupon,
                    PlazoId = IdPlazo,
                    Tasa = Tasa,
                    Estado = 1,
                    Usuario = Usuario.Trim().ToUpper()
                });

                if (spResp.pass == 0)
                {
                    response.Code = -1;
                    response.Description = spResp.mensaje ?? "Error al guardar la tasa por vencimiento.";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Actualizar Estado de CDP Tasa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodTasaRef"></param>
        /// <param name="Estado"></param>
        /// <param name="Notas"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto Fnd_CdpsTasas_Estado_Actualizar(int CodEmpresa, string CodTasaRef, bool Estado, string Notas, string Usuario)
        {
            const string sql = "exec spFnd_CDP_Tasas_Activacion @CodTasaRef, @Notas, @Usuario, @Activo";

            return DbHelper.ExecuteNonQuery(
                _portalDB,
                CodEmpresa,
                sql,
                new
                {
                    CodTasaRef,
                    Notas,
                    Usuario = Usuario.Trim().ToUpper(),
                    Activo = Estado ? 1 : 0
                });
        }

        /// <summary>
        /// Obtener Bitacora de CDP Tasa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodTasaRef"></param>
        /// <returns></returns>
        public ErrorDto<List<FndCdpsTasaBitacoraDto>> Fnd_CdpsTasas_Bitacora_Obtener(int CodEmpresa, string CodTasaRef)
        {
            const string query = @"exec spFnd_CDP_Tasa_Bitacora_Consulta @CodTasaRef";

            return DbHelper.ExecuteListQuery<FndCdpsTasaBitacoraDto>(
                _portalDB,
                CodEmpresa,
                query,
                new { CodTasaRef });
        }


        /// <summary>
        /// Eliminar CDP Tasa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodTasaRef"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto Fnd_CdpsTasas_Eliminar(int CodEmpresa, string CodTasaRef, string Usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sqlPlanes = "DELETE FROM FND_CDPS_TASA_PLANES WHERE COD_TASA_REF = @CodTasaRef";
                connection.Execute(sqlPlanes, new { CodTasaRef });

                const string sqlTasas = "DELETE FROM FND_CDP_TASACUPONES WHERE COD_TASA_REF = @CodTasaRef";
                connection.Execute(sqlTasas, new { CodTasaRef });

                const string sqlModelo = "DELETE FROM FND_CDP_TASAS_REF WHERE COD_TASA_REF = @CodTasaRef;";
                connection.Execute(sqlModelo, new { CodTasaRef });

                const string sqlSP = "exec spFndSeguridad_ApAnul_Delete @CodTasaRef, @Usuario";
                connection.Execute(sqlSP, new { CodTasaRef, Usuario });

                _mSecurity.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = Usuario.ToUpper(),
                    DetalleMovimiento = $"CDPS Modelo de Tasas: {CodTasaRef}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

    }
}
