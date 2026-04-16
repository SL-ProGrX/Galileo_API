using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivTiposDesembolsosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmVivTiposDesembolsosDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmVivTiposDesembolsosDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene tipos de desembolsos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<VivTiposDesembolsosData>> VivTiposDesembolsos_Obtener(int codEmpresa)
        {
            const string query = @"SELECT Codigo,Descripcion,NivelDesembolso,
                NivelFormalizacion,AplicaIngeniero,AplicaAbogado,
                AplicaInteres,Porcentaje,Cuenta,estado,
                RegistroUsuario,RegistroFecha 
            From ViviendaTiposDesembolsos order by descripcion";
            return DbHelper.ExecuteListQuery<VivTiposDesembolsosData>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Guarda tipo de desembolso
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto VivTiposDesembolsos_Guardar(int codEmpresa, int operacion, VivTiposDesembolsosData request)
        {
            const string sql = @"exec spCRDVivTiposDesembolsos_A @Operacion, @Codigo, @Descripcion, 
                @NivelDesembolso, @NivelFormalizacion, @AplicaIngeniero, @AplicaAbogado, @AplicaInteres, 
                @Porcentaje, @Cuenta, @Estado, @RegistroUsuario, @RegistroFecha";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Operacion = operacion,
                    Codigo = request.codigo,
                    Descripcion = request.descripcion?.Trim(),
                    NivelDesembolso = request.niveldesembolso ? 1 : 0,
                    NivelFormalizacion = request.nivelformalizacion ? 1 : 0,
                    AplicaIngeniero = request.aplicaingeniero ? 1 : 0,
                    AplicaAbogado = request.aplicaabogado ? 1 : 0,
                    AplicaInteres = request.aplicainteres ? 1 : 0,
                    Porcentaje = request.porcentaje,
                    Cuenta = request.cuenta?.Trim(),
                    Estado = request.estado,
                    RegistroUsuario = request.registrousuario,
                    RegistroFecha = request.registrofecha
                });

            if (resp.Code < 0)
                return resp;

            string movimiento = operacion == 1 ? "Modifica - WEB" : "Registra - WEB";

            RegistrarBitacora(
                codEmpresa,
                request.registrousuario,
                movimiento: movimiento,
                detalle: $"Vivienda tipo desembolso: {request.codigo}"
            );

            return resp;
        }

        /// <summary>
        /// Elimina tipo de desembolso
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto VivTiposDesembolsos_Eliminar(int codEmpresa, string codigo, string usuario)
        {
            const string sqlDelete = @"delete dbo.ViviendaTiposDesembolsos where codigo = @Codigo";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    Codigo = codigo.Trim()
                });
        }

        /// <summary>
        /// Registrar en bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
