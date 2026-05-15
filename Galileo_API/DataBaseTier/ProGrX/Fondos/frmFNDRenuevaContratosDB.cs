using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndRenuevaContratosDb
    {
        private readonly MProGrxMain _mProGrx_Main;
        private readonly PortalDB _portalDB;

        public FrmFndRenuevaContratosDb(IConfiguration config)
        {
            _mProGrx_Main = new MProGrxMain(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtener catálogo de renovación de contratos
        /// 0 - Operadoras 
        /// 1 - Estados Persona
        /// 2 - Planes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Index"></param>
        /// <param name="Operadora"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_RenuevaContratos_Catalogo_Obtener(int CodEmpresa, int Index, int Operadora)
        {
            string query = Index switch
            {
                // 0 - Operadoras
                0 => "select cod_operadora as item, RTRIM(DESCRIPCION) as descripcion from fnd_Operadoras",

                // 1 - Estados Persona 
                1 => "select rtrim(COD_ESTADO) as item, RTRIM(DESCRIPCION) as descripcion From AFI_ESTADOS_PERSONA Where ACTIVO = 1",

                // 2 - Planes 
                2 => $"select cod_plan as item, descripcion from fnd_planes where estado = 'A' And Cod_operadora = {Operadora} order by cod_plan",

                // Default → retorna vacío
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(query))
            {
                var response = new ErrorDto<List<DropDownListaGenericaModel>>();
                response.Code = -2;
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
        /// Obtener lista de renovación de contratos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<FndRenuevaContratosDto>> Fnd_ContratoRenueva_Obtener(int CodEmpresa, FndContratosBuscarParams filtros)
        {
            string sql = @"
                    SELECT
                        @aplicar_todos AS aplicar,
                        F.cedula       AS cedula,
                        S.nombre       AS nombre,
                        F.monto        AS monto,
                        dbo.fxFndExisteContratoPersona(F.cedula, @cod_operadora, @cod_plan_destino) AS existe,
                        F.cod_contrato AS cod_contrato
                    FROM fnd_contratos F
                    INNER JOIN socios S
                        ON F.cedula = S.cedula
                    WHERE
                        F.estado = 'A'
                        AND F.cod_plan = @cod_plan_origen
                        AND F.cod_operadora = @cod_operadora
                ";

            if (filtros.solo_renueva)
            {
                sql += " AND F.renueva = 'S' ";
            }

            if (!string.IsNullOrWhiteSpace(filtros.estado_socio) &&
                !string.Equals(filtros.estado_socio, "TODOS", StringComparison.OrdinalIgnoreCase))
            {
                sql += " AND S.EstadoActual = @estado_socio ";
            }

            return DbHelper.ExecuteListQuery<FndRenuevaContratosDto>(
                _portalDB,
                CodEmpresa,
                sql,
                filtros);
        }

        /// <summary>
        /// Aplicar renovación de contratos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Fnd_RenuevaContratos_Aplicar(int CodEmpresa, FndRenuevaContratosRequest request)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                var oficinas = _mProGrx_Main.CargaOficinas(request.usuario, CodEmpresa);
                string gOficinaTitular = oficinas?.FirstOrDefault()?.Titular ?? string.Empty;

                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"exec spFnd_RenuevaContratos 
                        @Operadora, 
                        @Codigo, 
                        @PlanDestino, 
                        @ContratoActual, 
                        @Plazo, 
                        @FechaVence, 
                        @Usuario, 
                        @OficinaTitular";

                foreach (var item in request.contratos)
                {
                    if (!item.existe)
                    {
                        connection.Execute(sql, new
                        {
                            Operadora = request.cod_operadora,
                            Codigo = request.cod_plan_origen.Trim(),
                            PlanDestino = request.cod_plan_destino.Trim(),
                            ContratoActual = item.cod_contrato,
                            Plazo = request.plazo,
                            FechaVence = request.fecha_vence,
                            Usuario = request.usuario.Trim().ToUpper(),
                            OficinaTitular = gOficinaTitular
                        });
                    }
                }

                response.Description = "Se generaron los fondos satisfactoriamente.";
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