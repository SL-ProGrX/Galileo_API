using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndTasaPreferencialAutorizacionDb
    {
        private readonly int vModulo = 18;
        private readonly MSecurityMainDb _securityMainDB;
        private readonly PortalDB _portalDB;

        public FrmFndTasaPreferencialAutorizacionDb(IConfiguration config)
        {
            _securityMainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtener Lista de Tasa Preferencial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="exporta"></param>
        /// <param name="data"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> Fnd_TasaPref_Obtener(int CodEmpresa, bool exporta, FndTasaPrefFiltros data, FiltrosLazyLoadData filtro)
        {
            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<FndTPListDto>()
                }
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                data ??= new FndTasaPrefFiltros();
                filtro ??= new FiltrosLazyLoadData();

                var param = new Dapper.DynamicParameters();

                var fechaInicio = (data.fecha_inicio ?? DateTime.Today).Date;
                var fechaCorte = (data.fecha_corte ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);

                param.Add("@FechaInicio", fechaInicio);
                param.Add("@FechaCorte", fechaCorte);

                var whereParts = new List<string>
                {
                    " Registro_Fecha BETWEEN @FechaInicio AND @FechaCorte"
                };

                if (data.estado != null)
                {
                    whereParts.Add("Estado = @pEstado");
                    param.Add("@pEstado", data.estado);
                }

                if (!string.IsNullOrWhiteSpace(data.usuario))
                {
                    whereParts.Add("Registro_Usuario LIKE @Usuario");
                    param.Add("@Usuario", $"%{data.usuario.Trim()}%");
                }

                if (!string.IsNullOrWhiteSpace(data.cedula))
                {
                    whereParts.Add("Cedula LIKE @Cedula");
                    param.Add("@Cedula", $"%{data.cedula.Trim()}%");
                }

                if (!string.IsNullOrWhiteSpace(data.nombre))
                {
                    whereParts.Add("Nombre LIKE @Nombre");
                    param.Add("@Nombre", $"%{data.nombre.Trim()}%");
                }

                if (!string.IsNullOrWhiteSpace(filtro.filtro))
                {
                    whereParts.Add(@"
                    (
                           ID_TP        LIKE @Filtro
                        OR ESTADO_DESC  LIKE @Filtro
                        OR Cedula       LIKE @Filtro
                        OR Nombre       LIKE @Filtro
                        OR Cod_Plan     LIKE @Filtro
                        OR Cod_Contrato LIKE @Filtro
                        OR Plan_Desc    LIKE @Filtro
                    )");
                    param.Add("@Filtro", $"%{filtro.filtro.Trim()}%");
                }

                string whereClause = "WHERE 1=1 " + string.Join(" AND ", whereParts);

                string sortField = (filtro.sortField ?? "").Trim();
                string orderByColumn = sortField.ToUpperInvariant() switch
                {
                    "ID_TP" => "ID_TP",
                    "ESTADO_DESC" => "ESTADO_DESC",
                    "CEDULA" => "Cedula",
                    "NOMBRE" => "Nombre",
                    "COD_PLAN" => "Cod_Plan",
                    "COD_CONTRATO" => "Cod_Contrato",
                    "PLAN_DESC" => "Plan_Desc",
                    "REGISTRO_FECHA" => "Registro_Fecha",
                    "REGISTRO_USUARIO" => "Registro_Usuario",
                    _ => "ID_TP"
                };

                string sortDirection = (filtro.sortOrder == 0) ? "DESC" : "ASC";

                int offset = filtro.pagina < 0 ? 0 : filtro.pagina;
                int fetch = filtro.paginacion <= 0 ? 10 : filtro.paginacion;

                param.Add("@Offset", offset);
                param.Add("@Fetch", fetch);

                var sqlCount = "SELECT COUNT(*) FROM vFnd_TP_List";
                if (!string.IsNullOrEmpty(whereClause))
                {
                    sqlCount += " " + whereClause;
                }
                response.Result.total = connection.QueryFirstOrDefault<int>(sqlCount, param);


                string sqlLista = $@"SELECT * FROM vFnd_TP_List {whereClause}
                    ORDER BY {orderByColumn} {sortDirection}";

                if (!exporta)
                {
                    sqlLista += " OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY";
                }

                response.Result.lista = connection.Query<FndTPListDto>(sqlLista, param).ToList();
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
        /// Autorizar o Denegar Tasa Preferencial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Gestion"></param>
        /// <param name="Autorizador"></param>
        /// <param name="Gestiones"></param>
        /// <returns></returns>
        public ErrorDto Fnd_TasaPref_Autorizar(int CodEmpresa, string Gestion, string Autorizador, List<FndTPListDto> Gestiones)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                string estadoTexto = (Gestion == "A") ? "Autoriza" : "Deniega";
                var msjError = new System.Text.StringBuilder();

                const string query = @"exec spFnd_TP_Autorizacion @GestionId, @Gestion, @Usuario";

                foreach (var item in Gestiones)
                {
                    try
                    {
                        connection.Execute(query, new
                        {
                            GestionId = item.id_tp,
                            Gestion,
                            Usuario = Autorizador
                        });

                        _securityMainDB.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Autorizador,
                            Movimiento = "Aplica - WEB",
                            Modulo = vModulo,
                            DetalleMovimiento = $"{estadoTexto} Tasa Preferencial " +
                                $"Gestion Id:{item.id_tp} ..Id: {item.cedula} ..Nombre: {item.nombre}"
                        });
                    }
                    catch (Exception exItem)
                    {
                        msjError.AppendLine($"Error en la gestión {item.id_tp}: {exItem.Message}");
                    }
                }

                if (msjError.Length > 0)
                {
                    response.Code = -1;
                    response.Description = msjError.ToString();
                }
                else
                {
                    response.Description = "Proceso realizado satisfactoriamente.!";
                }
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
