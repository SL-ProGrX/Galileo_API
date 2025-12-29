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

                var whereClause = $@"WHERE Estado = @pEstado 
                    AND Registro_Fecha BETWEEN '{data.fecha_inicio:yyyy-MM-dd} 00:00:00'
                    AND '{data.fecha_corte:yyyy-MM-dd} 23:59:59'";

                if (!string.IsNullOrEmpty(data.usuario))
                {
                    whereClause += $@" AND Registro_Usuario LIKE '%{data.usuario}%'";
                }

                if (!string.IsNullOrEmpty(data.cedula))
                {
                    whereClause += $@" AND Cedula LIKE '%{data.cedula}%'";
                }

                if (!string.IsNullOrEmpty(data.nombre))
                {
                    whereClause += $@" AND Nombre LIKE '%{data.nombre}%'";
                }

                string sql = "SELECT COUNT(*) FROM vFnd_TP_List WHERE 1=1 ";

                if (data.estado != null)
                {
                    sql += " AND Estado = @pEstado ";
                }

                response.Result.total = connection.QueryFirstOrDefault<int>(
                    sql.ToString(),
                    new { pEstado = data.estado }
                );

                if (!string.IsNullOrEmpty(filtro.filtro))
                {
                    var f = filtro.filtro;
                    filtro.filtro = $@" AND (
                                      ID_TP           LIKE '%{f}%'
                                   OR ESTADO_DESC     LIKE '%{f}%'
                                   OR Cedula          LIKE '%{f}%'
                                   OR Nombre          LIKE '%{f}%'
                                   OR Cod_Plan        LIKE '%{f}%'
                                   OR Cod_Contrato    LIKE '%{f}%'
                                   OR Plan_Desc       LIKE '%{f}%'
                               )";
                }

                if (string.IsNullOrEmpty(filtro.sortField))
                {
                    filtro.sortField = "ID_TP";
                }

                string sqlLista;

                if (exporta)
                {
                    sqlLista = $@"SELECT * FROM vFnd_TP_List  {whereClause} {filtro.filtro}
                    ORDER BY {filtro.sortField} {(filtro.sortOrder == 0 ? "DESC" : "ASC")}";
                }
                else
                {
                    sqlLista = $@" SELECT * FROM vFnd_TP_List {whereClause} {filtro.filtro}
                    ORDER BY {filtro.sortField} {(filtro.sortOrder == 0 ? "DESC" : "ASC")}
                    OFFSET {filtro.pagina} ROWS
                    FETCH NEXT {filtro.paginacion} ROWS ONLY";
                }

                response.Result.lista = connection.Query<FndTPListDto>(
                    sqlLista,
                    new { pEstado = data.estado }
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
