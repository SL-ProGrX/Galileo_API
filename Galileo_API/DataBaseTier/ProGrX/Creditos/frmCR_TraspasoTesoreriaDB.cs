using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ProGrX.Credito;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Credito
{
    public class FrmCRTraspasoTesoreriaDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 3; // Modulo de Créditos
        private readonly MTesoreria _mtes;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCRTraspasoTesoreriaDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mtes = new MTesoreria(_config);
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        #region remesas
        #endregion

        #region cargar
        #endregion

        #region trasladar

        /// <summary>
        /// Método para obtener las remesas en estado 'C' (Cerradas) para el traspaso a tesoreria
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_TraspasoTes_Remesas_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                @"select cod_remesa as item,
                         CONCAT(cod_remesa,' - ',FECHA_INICIO,' - ', FECHA_CORTE, ' - ', USUARIO ) as descripcion
                  from CRD_REMESAS_TES
                  where estado = 'C'
                  order by fecha desc");
        }

        /// <summary>
        /// Método: Obtiene los tokens disponibles para la liquidación de afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<TokenConsultaModel>> Cr_TraspasoTesToken_Obtener(int CodEmpresa, string usuario)
        {
            return _mtes.spTes_Token_Consulta(CodEmpresa, usuario);
        }

        /// <summary>
        /// Método: Genera un nuevo token para la liquidación de afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Cr_TraspasoTesToken_Nuevo(int CodEmpresa, string usuario)
        {
            return _mtes.spTes_Token_New(CodEmpresa, usuario);
        }

        public ErrorDto<List<TraspasoModel>> Cr_TraspasoTesTraslado_Buscar(int CodEmpresa, int cod_remesa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var remesa = connection.QueryFirstOrDefault<(DateTime fecha_inicio, DateTime fecha_corte)?>(
                    "select fecha_inicio, fecha_corte from CRD_REMESAS_TES where cod_remesa = @cod_remesa",
                    new { cod_remesa });

                if (!remesa.HasValue)
                {
                    return DbHelper.CreateErrorResponse("No se encontró la remesa.", -1, new List<TraspasoModel>());
                }

                var fechaInicio = remesa.Value.fecha_inicio.Date;
                var fechaCorte = remesa.Value.fecha_corte.Date.AddDays(1).AddTicks(-1);

                var lista = connection.Query<TraspasoModel>(
                    @"select R.id_solicitud,
                             R.codigo,
                             S.cedula,
                             S.nombre,
                             R.montoapr,
                             R.monto_girado,
                             isnull(D.Numero,0) as Desembolsos_Numero,
                             isnull(D.Monto,0) as Desembolsos
                      from reg_creditos R
                      inner join Socios S on R.cedula = S.cedula
                      inner join Catalogo C on R.codigo = C.codigo and C.retencion = 'N' and C.poliza = 'N'
                      left join vCrdOperacion_DesembolsosGiro D on R.id_Solicitud = D.id_Solicitud
                      where R.estadosol = 'F'
                        and R.fechaforp between @fechaInicio and @fechaCorte
                        and R.estado in('A','C')
                        and R.id_solicitud in(
                            select id_solicitud
                            from CRD_REMESAS_TES_DETALLE
                            where cod_remesa = @id_remesa)
                      order by R.id_solicitud",
                    new
                    {
                        fechaInicio,
                        fechaCorte,
                        id_remesa = cod_remesa
                    }).ToList();

                return DbHelper.CreateOkResponse(lista);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar operaciones para traslado a tesorería.", result.Code.GetValueOrDefault(-1), new List<TraspasoModel>());
        }

        public ErrorDto CrTraspasoTes_Traslado_Generar(int CodEmpresa, int cod_remesa, string usuario, string? token)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var tokenTrabajo = NormalizarToken(token);
                if (string.IsNullOrWhiteSpace(tokenTrabajo))
                {
                    tokenTrabajo = ObtenerOTokenGenerado(CodEmpresa, usuario, connection);
                }

                var lista = connection.Query<(int id_solicitud, string codigo)>(
                    @"select id_solicitud, codigo
                      from reg_creditos
                      where estado in('A','C')
                        and estadosol = 'F'
                        and tesoreria is null
                        and id_solicitud in(
                            select id_solicitud
                            from CRD_REMESAS_TES_DETALLE
                            where cod_remesa = @cod_remesa)",
                    new { cod_remesa }).ToList();

                foreach (var item in lista)
                {
                    connection.Execute(
                        "exec spCrdCreditoEnviaTesoreria_Todo @Operacion, @Token, @Remesa, @RemesaTipo",
                        new
                        {
                            Operacion = item.id_solicitud,
                            Token = tokenTrabajo,
                            Remesa = cod_remesa,
                            RemesaTipo = "CRD"
                        });

                    RegistrarBitacora(
                        CodEmpresa,
                        usuario,
                        $"Traspaso a Tesoreria de la Operacion y Desembol OP: {item.id_solicitud}",
                        "Registra - WEB");

                    _mtes.sbCrdOperacionTags(
                        CodEmpresa,
                        item.id_solicitud,
                        item.codigo,
                        "S04",
                        usuario,
                        string.Empty,
                        $"Remesa de Traslado No..: {cod_remesa}");
                }

                connection.Execute(
                    "update CRD_REMESAS_TES SET Estado = 'T' Where cod_remesa = @cod_remesa",
                    new { cod_remesa });

                return DbHelper.OkResponse("Operaciones Enviadas a Tesoreria Satisfactoriamente...");
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al generar traspaso a tesorería.", result.Code.GetValueOrDefault(-1));
        }

        #endregion

        #region informes
        #endregion

        #region reactivaciones
        #endregion

        #region cambio
        #endregion

        #region consultas
        #endregion

        #region aux.giro
        #endregion

        private string ObtenerOTokenGenerado(int codEmpresa, string usuario, SqlConnection connection)
        {
            const string queryToken = "select top 1 id_token from tes_tokens where estado = 'A' order by registro_fecha";
            var token = connection.QueryFirstOrDefault<string?>(queryToken);
            if (!string.IsNullOrWhiteSpace(token))
            {
                return token.Trim();
            }

            _mtes.spTes_Token_New(codEmpresa, usuario);
            return connection.QueryFirstOrDefault<string?>(queryToken)?.Trim() ?? string.Empty;
        }

        private static string? NormalizarToken(string? token)
        {
            return string.IsNullOrWhiteSpace(token) ? null : token.Trim();
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}