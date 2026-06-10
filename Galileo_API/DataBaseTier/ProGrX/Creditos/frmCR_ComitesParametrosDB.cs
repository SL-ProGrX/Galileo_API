namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    using Galileo.DataBaseTier;
    using Galileo.Models.ERROR;
    using Galileo.Models.Security;
    using Galileo_API.Models.ProGrX.Creditos;

    public class FrmCrComitesParametrosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Bitacora;
        private readonly int vModulo = 3;

        public FrmCrComitesParametrosDB(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmCrComitesParametrosDB(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _Bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene el catálogo de parámetros de comités.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComitesParametroModel>> CrComitesParametros_Obtener(int CodEmpresa)
        {
            const string sqlQuery = @"
                select 
                    cod_parametro as Cod_Parametro,
                    descripcion as Descripcion,
                    valor as Valor
                from crd_comites_parametros 
                order by cod_parametro";

            return DbHelper.ExecuteListQuery<CrComitesParametroModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery);
        }

        /// <summary>
        /// Actualiza un parámetro de comités.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrComitesParametros_Actualizar(int CodEmpresa, CrComitesParametroActualizarRequest request)
        {
            const string sqlQuery = @"
                exec spCrd_Comites_Parametro_Actualiza 
                    @CodParametro, 
                    @Valor, 
                    @Usuario";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                new
                {
                    CodParametro = (request.Cod_Parametro ?? string.Empty).Trim(),
                    Valor = (request.Valor ?? string.Empty).Trim(),
                    Usuario = (request.Usuario ?? string.Empty).Trim()
                });

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                CodEmpresa,
                request.Usuario ?? "",
                movimiento: "Modifica - WEB",
                detalle: $"Parametro de Comite : {request.Cod_Parametro}");

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Registrar en bitacora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int CodEmpresa, string usuario, string movimiento, string detalle)
        {
            _Bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
