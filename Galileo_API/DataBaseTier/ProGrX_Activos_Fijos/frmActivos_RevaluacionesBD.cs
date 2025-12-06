using Galileo.Models.ERROR;
using Dapper;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosRevaluacionesDB
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;
        public FrmActivosRevaluacionesDB(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método de Guardar la revaluacion de un activo fijo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Activos_Revaluaciones_Guardar(int CodEmpresa, string usuario, ActivosRevaluacionData data)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok",
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"exec spActivos_AdicionRetiro @Placa,'V',@Justificacion,@Descripcion,@Fecha,@Monto,@Meses,@Usuario, '','','',''";
                int Linea = connection.Query<int>(query, new
                {
                    Placa = data.num_placa,
                    Justificacion = data.cod_justificacion,
                    Descripcion = data.descripcion,
                    Fecha = data.fecha,
                    Monto = data.monto,
                    Meses = data.meses_calculo,
                    Usuario = usuario
                }).FirstOrDefault();

                result.Code = Linea;

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Revaluación (Placa: {data.num_placa}) Id: {data.id_addret}:{data.id_addret}_ {data.justificacion} ",
                    Movimiento = "Registra - WEB",
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


        /// <summary>
        /// Método para consultar el histórico de revaluaciones de un activo fijo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosHistoricoData>> Activos_Revaluaciones_Historico_Consultar(int CodEmpresa, string placa)
        {
            var result = new ErrorDto<List<ActivosHistoricoData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosHistoricoData>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select X.id_AddRet,x.fecha,x.MONTO,x.DESCRIPCION,rtrim(J.cod_justificacion) + '..' + J.descripcion as Justifica                                 
                                 ,A.nombre,P.cod_proveedor,P.descripcion as Proveedor, 'Revaluación' as tipoMov
                              from Activos_retiro_adicion X inner join Activos_Principal A on X.num_placa = A.num_placa
                             inner join Activos_justificaciones J on X.cod_justificacion = J.cod_justificacion
                               left join Activos_proveedores P on X.compra_proveedor = P.cod_proveedor
                                 where X.num_placa = @placa and X.Tipo = 'V'";
                result.Result = connection.Query<ActivosHistoricoData>(query, new
                { placa }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        ///  Metodo para eliminar una revaluacion de un activo fijo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <param name="Id_AddRet"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Activos_Revaluaciones_Eliminar(int CodEmpresa, string placa, int Id_AddRet, string usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"delete Activos_retiro_adicion where num_placa = @placa and Id_AddRet = @Id_AddRet";
                connection.Execute(query, new { placa, Id_AddRet });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Revaluación, Placa: {placa}) Id: {Id_AddRet}",
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