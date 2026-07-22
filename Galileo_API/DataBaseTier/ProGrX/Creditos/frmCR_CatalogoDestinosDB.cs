using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCatalogoDestinosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Bitacora;
        private readonly int vModulo = 3;

        public FrmCrCatalogoDestinosDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmCrCatalogoDestinosDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _Bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene la lista de catalogo destinos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoDestinoData>> CrCatalogoDestinos_Obtener(int codEmpresa)
        {
            string query = @"select cod_destino,descripcion,tasa,tbp,int_form,
            case when isnull(TCIntForma,'A') = 'A' then 'Adelantado' else 'Vencido' end as 'TipoCbrInt', 
            primer_cuota,ENVIO_TESORERIA,
            case
                when isnumeric(ltrim(rtrim(convert(varchar(20), isnull(prioridad, ''))))) = 1
                    then convert(int, ltrim(rtrim(convert(varchar(20), prioridad))))
                else 0
            end as prioridad
            from Catalogo_Destinos 
            order by cod_destino";
            return DbHelper.ExecuteListQuery<CrCatalogoDestinoData>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de catalogos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCatalogos_Obtener(int codEmpresa, string tipo)
        {
            string query = @"select codigo as item,descripcion from catalogo";
            if (tipo == "N") { 
                query += @" where (Retencion = 'N' and Poliza = 'N') order by codigo";
            } else {
                query += @" where (Retencion = 'S' or Poliza = 'S') order by codigo";
            }
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de destinos asignados a un catalogo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoDestinoData>> CrCatalogoDestinos_Asignados_Obtener(int codEmpresa, string codigo)
        {
            string query = @"SELECT
                R.cod_destino,
                R.descripcion,
                R.tasa,
                R.tbp,
                R.int_form,
                case when isnull(R.TCIntForma,'A') = 'A' then 'Adelantado' else 'Vencido' end as TipoCbrInt,
                R.primer_cuota,
                R.ENVIO_TESORERIA,
                case
                    when isnumeric(ltrim(rtrim(convert(varchar(20), isnull(R.prioridad, ''))))) = 1
                        then convert(int, ltrim(rtrim(convert(varchar(20), R.prioridad))))
                    else 0
                end as prioridad,
                CASE 
                    WHEN A.codigo IS NOT NULL THEN 1
                    ELSE 0
                END AS Existe
            FROM Catalogo_Destinos R
            LEFT JOIN Catalogo_DestinosAsg A 
                ON R.cod_destino = A.cod_destino 
               AND A.codigo = @codigo
            ORDER BY Existe DESC, R.cod_destino;";
            return DbHelper.ExecuteListQuery<CrCatalogoDestinoData>(_portalDb, codEmpresa, query, new { codigo });
        }

        /// <summary>
        /// Asigna o desasigna un destino a un catalogo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codDestino"></param>
        /// <param name="catalogo"></param>
        /// <param name="isChecked"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoDestinos_Asignar(int codEmpresa, string codDestino, string catalogo, bool isChecked)
        {
            string sql = isChecked
                ? @"
            INSERT INTO Catalogo_DestinosAsg (cod_destino, codigo)
            VALUES (@CodDestino, @Codigo);"
                : @"
            DELETE FROM Catalogo_DestinosAsg
            WHERE cod_destino = @CodDestino
              AND codigo = @Codigo;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodDestino = codDestino.Trim(),
                    Codigo = catalogo.Trim()
                }
            );
        }

        /// <summary>
        /// Guarda un destino, actualiza o agrega dependiendo si existe
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoDestinos_Guardar(int codEmpresa, string usuario, CrCatalogoDestinoData request)
        {
            request.cod_destino = request.cod_destino.Trim();
            request.descripcion = request.descripcion?.Trim() ?? string.Empty;
            request.tipocbrint = request.tipocbrint?.Trim() ?? string.Empty;

            var existe = ExisteDestino(codEmpresa, request.cod_destino);

            var resp = existe
                ? ActualizarDestino(codEmpresa, usuario, request)
                : InsertarDestino(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Elimina un destino del catalogo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codDestino"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoDestinos_Eliminar(int codEmpresa, string codDestino, string usuario)
        {
            const string sqlDelete = @"DELETE FROM Catalogo_Destinos
            WHERE cod_destino = @CodDestino;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodDestino = codDestino.Trim()
                }
            );

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Elimina - WEB",
                detalle: $"Destino de Línea Cod: {codDestino}"
            );

            return respDelete;
        }

        /// <summary>
        /// Actualiza datos de un destino
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ActualizarDestino(int codEmpresa, string usuario, CrCatalogoDestinoData request)
        {
            const string sqlUpdate = @"
            UPDATE Catalogo_Destinos
            SET
                descripcion       = @Descripcion,
                tasa              = @Tasa,
                TBP               = @TBP,
                int_form          = @IntForm,
                TCIntForma        = @TipoCbrInt,
                primer_cuota      = @PrimerCuota,
                envio_tesoreria   = @EnvioTesoreria,
                prioridad         = @Prioridad
            WHERE cod_destino = @CodDestino;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodDestino = request.cod_destino,
                    Descripcion = request.descripcion,
                    Tasa = request.tasa,
                    TBP = request.tbp ? 1 : 0,
                    IntForm = request.int_form ? 1 : 0,
                    TipoCbrInt = request.tipocbrint.First(),
                    PrimerCuota = request.primer_cuota ? 1 : 0,
                    EnvioTesoreria = request.envio_tesoreria ? 1 : 0,
                    Prioridad = request.prioridad
                }
            );

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Modifica - WEB",
                detalle: $"Destino Línea Cod: {request.cod_destino}"
            );

            return respUpdate;
        }

        /// <summary>
        /// Inserta un nuevo destino
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarDestino(int codEmpresa, string usuario, CrCatalogoDestinoData request)
        {
            const string sqlInsert = @"
            INSERT INTO Catalogo_Destinos
            (
                cod_destino,
                descripcion,
                tasa,
                TBP,
                int_form,
                TCIntForma,
                primer_cuota,
                envio_tesoreria,
                prioridad
            )
            VALUES
            (
                @CodDestino,
                @Descripcion,
                @Tasa,
                @TBP,
                @IntForm,
                @TipoCbrInt,
                @PrimerCuota,
                @EnvioTesoreria,
                @Prioridad
            );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodDestino = request.cod_destino,
                    Descripcion = request.descripcion,
                    Tasa = request.tasa,
                    TBP = request.tbp ? 1 : 0,
                    IntForm = request.int_form ? 1 : 0,
                    TipoCbrInt = request.tipocbrint.First(),
                    PrimerCuota = request.primer_cuota ? 1 : 0,
                    EnvioTesoreria = request.envio_tesoreria ? 1 : 0,
                    Prioridad = request.prioridad
                }
            );

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Registra - WEB",
                detalle: $"Destino de Línea Cod: {request.cod_destino}"
            );

            return respInsert;
        }

        /// <summary>
        /// Valida si el destino existe
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codDestino"></param>
        /// <returns></returns>
        private bool ExisteDestino(int codEmpresa, string codDestino)
        {
            const string sqlExiste = @"SELECT ISNULL(COUNT(*), 0) 
            FROM Catalogo_Destinos WHERE cod_destino = @CodDestino;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb, codEmpresa, sqlExiste, 0,
                new
                {
                    CodDestino = codDestino.Trim()
                }
            );
            return resp.Result > 0;
        }

        /// <summary>
        /// Registra en bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _Bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
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
