using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed class FrmIvrCatInstrumentosDb
    {
        private const int VModulo = 22; // Módulo de Inversiones (vModulo del formulario VB6)
        private const string GuardadoExitoso = "Informacion guardada satisfactoriamente...";
        private const string EliminadoExitoso = "Informacion eliminada satisfactoriamente...";
        private const string CodigoRequerido = "Debe indicar el código del instrumento.";

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;

        public FrmIvrCatInstrumentosDb(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene los tipos de instrumentos de inversión.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<IvrCatInstrumentoData>> IvrCatInstrumentos_Obtener(int codEmpresa)
        {
            const string sqlQuery = @"
                select
                    rtrim(COD_INSTRUMENTO) as cod_instrumento,
                    rtrim(isnull(DESCRIPCION, '')) as descripcion,
                    left(isnull(TIPO_DESC, ''), 1) as tipo,
                    case when charindex(' - ', isnull(Form_ItmX, '')) > 0
                         then ltrim(rtrim(left(Form_ItmX, charindex(' - ', Form_ItmX) - 1)))
                         else rtrim(isnull(Form_ItmX, '')) end as tipo_form,
                    cast(isnull(BASE_INTERESES, 0) as decimal(18, 4)) as base_intereses,
                    cast(isnull(BASE_DIAS, 0) as int) as base_dias,
                    cast(isnull(VALORIZA, 0) as bit) as valoriza,
                    cast(isnull(DIAS_VALORIZA, 0) as int) as dias_valoriza,
                    cast(isnull(ACTIVO, 0) as bit) as activo
                from vIVR_INSTRUMENTOS
                order by COD_INSTRUMENTO;";

            return DbHelper.ExecuteListQuery<IvrCatInstrumentoData>(
                _portalDb,
                codEmpresa,
                sqlQuery
            );
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un instrumento.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto IvrCatInstrumentos_Guardar(
            int codEmpresa,
            IvrCatInstrumentoGuardarRequest request)
        {
            request.usuario = (request.usuario ?? string.Empty).Trim();
            request.instrumento.cod_instrumento =
                (request.instrumento.cod_instrumento ?? string.Empty).Trim().ToUpperInvariant();
            request.instrumento.descripcion = (request.instrumento.descripcion ?? string.Empty).Trim();
            request.instrumento.tipo = (request.instrumento.tipo ?? string.Empty).Trim();
            request.instrumento.tipo_form = (request.instrumento.tipo_form ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.instrumento.cod_instrumento))
            {
                return new ErrorDto { Code = -1, Description = CodigoRequerido };
            }

            var existe = ExisteInstrumento(codEmpresa, request.instrumento.cod_instrumento);

            if (existe.Code < 0)
            {
                return new ErrorDto { Code = existe.Code, Description = existe.Description };
            }

            return existe.Result > 0
                ? ActualizarInstrumento(codEmpresa, request)
                : InsertarInstrumento(codEmpresa, request);
        }

        /// <summary>
        /// Elimina un instrumento.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto IvrCatInstrumentos_Eliminar(
            int codEmpresa,
            IvrCatInstrumentoEliminarRequest request)
        {
            var usuario = (request.usuario ?? string.Empty).Trim();
            var codigo = (request.cod_instrumento ?? string.Empty).Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto { Code = -1, Description = CodigoRequerido };
            }

            const string sqlDelete = @"
                delete from IVR_INSTRUMENTOS
                where COD_INSTRUMENTO = @Codigo;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new { Codigo = codigo }
            );

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(codEmpresa, usuario, "Elimina - WEB", codigo);

            return new ErrorDto { Code = 0, Description = EliminadoExitoso };
        }

        private ErrorDto InsertarInstrumento(int codEmpresa, IvrCatInstrumentoGuardarRequest request)
        {
            const string sqlInsert = @"
                insert into IVR_INSTRUMENTOS
                (
                    COD_INSTRUMENTO, DESCRIPCION, TIPO, TIPO_FORM, BASE_INTERESES, BASE_DIAS,
                    VALORIZA, DIAS_VALORIZA, ACTIVO, NOTAS, REGISTRO_USUARIO, REGISTRO_FECHA
                )
                values
                (
                    @Codigo, @Descripcion, @Tipo, @TipoForm, @BaseIntereses, @BaseDias,
                    @Valoriza, @DiasValoriza, @Activo, '', @Usuario, getdate()
                );";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                CrearParametros(request)
            );

            return FinalizarGuardado(codEmpresa, request, "Registra - WEB", resp);
        }

        private ErrorDto ActualizarInstrumento(int codEmpresa, IvrCatInstrumentoGuardarRequest request)
        {
            const string sqlUpdate = @"
                update IVR_INSTRUMENTOS
                set DESCRIPCION = @Descripcion,
                    TIPO = @Tipo,
                    TIPO_FORM = @TipoForm,
                    BASE_INTERESES = @BaseIntereses,
                    BASE_DIAS = @BaseDias,
                    VALORIZA = @Valoriza,
                    DIAS_VALORIZA = @DiasValoriza,
                    ACTIVO = @Activo,
                    MODIFICA_FECHA = getdate(),
                    MODIFICA_USUARIO = @Usuario
                where COD_INSTRUMENTO = @Codigo;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                CrearParametros(request)
            );

            return FinalizarGuardado(codEmpresa, request, "Modifica - WEB", resp);
        }

        private static object CrearParametros(IvrCatInstrumentoGuardarRequest request)
        {
            var i = request.instrumento;

            return new
            {
                Codigo = i.cod_instrumento,
                Descripcion = i.descripcion,
                Tipo = i.tipo.Length > 0 ? i.tipo[..1] : string.Empty,
                TipoForm = i.tipo_form,
                BaseIntereses = i.base_intereses,
                BaseDias = i.base_dias,
                Valoriza = i.valoriza ? 1 : 0,
                DiasValoriza = i.dias_valoriza,
                Activo = i.activo ? 1 : 0,
                Usuario = request.usuario
            };
        }

        private ErrorDto FinalizarGuardado(
            int codEmpresa,
            IvrCatInstrumentoGuardarRequest request,
            string movimiento,
            ErrorDto resp)
        {
            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(codEmpresa, request.usuario, movimiento, request.instrumento.cod_instrumento);

            return new ErrorDto { Code = 0, Description = GuardadoExitoso };
        }

        private ErrorDto<int> ExisteInstrumento(int codEmpresa, string codigo)
        {
            const string sqlExiste = @"
                select isnull(count(*), 0) as Existe
                from IVR_INSTRUMENTOS
                where COD_INSTRUMENTO = @Codigo;";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new { Codigo = codigo }
            );
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string codigo)
        {
            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Tipo de Instrumento:  {codigo}",
                Movimiento = movimiento,
                Modulo = VModulo
            });
        }
    }
}
