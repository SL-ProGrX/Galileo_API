using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace Galileo.DataBaseTier.ProGrX.Clientes;

public partial class FrmAFCrenunciaDB
{
    /// <summary>Obtiene las autorizaciones del formulario legacy.</summary>
    public ErrorDto<AfRenunciaConfiguracion> AF_CR_Renuncias_Configuracion_Obtener(int CodEmpresa, string usuario) =>
        DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => connection.QuerySingle<AfRenunciaConfiguracion>(
            "SELECT CAST(CASE WHEN (SELECT VALOR FROM SIF_PARAMETROS WHERE COD_PARAMETRO = 'PAT_R') = 'S' THEN 1 ELSE 0 END AS bit) aporte_patronal, dbo.fxAFI_Renuncia_Arreglo_Pago(@usuario) arreglo_pago",
            new { usuario }));

    /// <summary>Valida los requisitos de afiliación al marcar reingreso.</summary>
    public ErrorDto<List<string>> AF_CR_Renuncias_Reingreso_Validar(int CodEmpresa, string cedula, string usuario)
    {
        var globales = new MProGrxMain(_config).sbSifParametrosInicializa(CodEmpresa, usuario);
        if (globales.Code != 0 || globales.Result == null)
            return DbHelper.CreateErrorResponse<List<string>>(globales.Description ?? "No fue posible obtener los parámetros.");
        return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
        {
            var row = connection.QuerySingleOrDefault(@"SELECT S.*,
                dbo.fxAFI_Afiliacion_Valida_Beneficiarios(S.Cedula) beneficiarios_validos,
                dbo.fxAFI_Afiliacion_Valida_Telefonos(S.Cedula) telefonos_validos,
                dbo.fxAFI_Afiliacion_Valida_Beneficiarios_MenoresSinAlbacea(S.Cedula) menores_sin_albacea,
                GETDATE() fecha_sistema FROM socios S WHERE S.Cedula = @cedula", new { cedula });
            if (row == null) return new List<string> { "La persona especificada no existe." };
            return AF_CR_Renuncias_Reingreso_Requisitos((IDictionary<string, object>)row, globales.Result.SysASEVersion);
        });
    }

    private static List<string> AF_CR_Renuncias_Reingreso_Requisitos(IDictionary<string, object> row, bool ase)
    {
        var data = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
        string Texto(string key) => Convert.ToString(data.GetValueOrDefault(key))?.Trim() ?? "";
        decimal Numero(string key) => decimal.TryParse(Texto(key), out var value) ? value : 0;
        DateTime Fecha(string key, DateTime fallback) => DateTime.TryParse(Texto(key), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value) ? value : fallback;
        var hoy = Fecha("fecha_sistema", DateTime.Today);
        var salario = Numero("SALARIO_MONTO");
        var local = string.IsNullOrEmpty(Texto("SALARIO_DIVISA")) || Texto("SALARIO_DIVISA") == "COL";
        var reglas = new (bool Error, string Mensaje)[]
        {
            (Fecha("FECHA_NAC", hoy) > hoy.AddYears(-17), "Verifique la fecha de nacimiento, la persona es menor de edad."),
            (Fecha("FECHA_VEN_CED", hoy) <= hoy.AddDays(20), "El documento de identidad está pronto a vencer."),
            (Numero("cod_profesion") == 0, "Profesión no válida."),
            (Texto("EstadoLaboral") == "", "No se especificó el estado laboral."),
            (Numero("NIVEL_ACADEMICO") == 0, "No se especificó el nivel académico."),
            (Numero("ACTIVIDADES") == 0, "No se especificó la actividad de cumplimiento."),
            (ase && Texto("cod_departamento") + Texto("UP") == "", "No se especificó departamento o unidad programática."),
            (Texto("COD_CARGO") == "", "Debe indicar el puesto que desempeña."),
            (Numero("beneficiarios_validos") == 0 && Numero("I_BENEFICIARIOS") == 1, "Los beneficiarios están incompletos."),
            (!MAfilicacionDB.fxEmail_Validar(Texto("AF_Email")), "El correo principal no es válido."),
            (Texto("Email_02") != "" && !MAfilicacionDB.fxEmail_Validar(Texto("Email_02")), "El correo secundario no es válido."),
            (salario < (local ? 100000 : 200) || salario > (local ? 10000000 : 20000), "El salario devengado no es válido."),
            (Texto("Provincia") == "", "No se especificó provincia."),
            (Texto("Canton") == "", "No se especificó cantón."),
            (Texto("Distrito") == "", "No se especificó distrito."),
            (!MAfilicacionDB.fxDireccion_Validar(new MAfilicacionDireccionValidarRequest { direccion = Texto("Direccion"), caracteres_relleno = "-,#,*" }), "La dirección no es válida."),
            (Numero("menores_sin_albacea") > 0 && Numero("I_BENEFICIARIOS") == 1 && Texto("Albacea_cedula").Length <= 5, "Existen beneficiarios menores sin albacea."),
            (Numero("telefonos_validos") == 0, "No se han registrado teléfonos de contacto.")
        };
        return reglas.Where(x => x.Error).Select(x => x.Mensaje).ToList();
    }

    /// <summary>Registra encabezado, motivos, todos los planes y abonos en una transacción.</summary>
    public ErrorDto<int> AF_CR_Renuncias_Proceso_Guardar(int CodEmpresa, AfRenunciaProceso request)
    {
        var reingresoError = AF_CR_Renuncias_Reingreso_Error(CodEmpresa, request);
        if (reingresoError != null) return reingresoError;

        var globales = new MProGrxMain(_config).sbSifParametrosInicializa(CodEmpresa, request.Usuario);
        if (globales.Code != 0 || globales.Result == null)
            return DbHelper.CreateErrorResponse<int>(globales.Description ?? "No fue posible obtener la oficina titular.");
        request.Oficina = globales.Result.GOficinaTitular ?? "";
        return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
        {
            if (connection.State != ConnectionState.Open) connection.Open();
            using var transaction = connection.BeginTransaction();
            AF_CR_Renuncias_Proceso_Validar(connection, transaction, request);
            AF_CR_Renuncias_Importes_Validar(connection, transaction, request);
            var id = AF_CR_Renuncias_Liquidacion_Guarda_Ejecutar(connection, request, transaction);
            foreach (var motivo in request.Motivos.Distinct())
                connection.Execute("exec spAFI_CR_Motivos_Registra @id, @motivo, 'A', @usuario", new { id, motivo, usuario = request.Usuario }, transaction);
            foreach (var plan in request.Planes)
            {
                plan.CodRenuncia = id;
                AF_CR_Renuncias_Plan_Insertar_Ejecutar(connection, plan, transaction);
            }
            foreach (var abono in request.Abonos)
            {
                abono.CodRenuncia = id;
                AF_CR_Renuncias_Abono_Insertar_Ejecutar(connection, abono, transaction);
            }
            transaction.Commit();
            return id;
        });
    }

    private ErrorDto<int>? AF_CR_Renuncias_Reingreso_Error(int CodEmpresa, AfRenunciaProceso request)
    {
        if (request.Reingreso != true) return null;

        var validacion = AF_CR_Renuncias_Reingreso_Validar(CodEmpresa, request.Cedula, request.Usuario);
        if (validacion.Code != 0 || validacion.Result == null)
            return DbHelper.CreateErrorResponse<int>(validacion.Description ?? "No fue posible validar el reingreso.");
        return validacion.Result.Count == 0
            ? null
            : DbHelper.CreateErrorResponse<int>(string.Join(Environment.NewLine, validacion.Result));
    }

    private static void AF_CR_Renuncias_Proceso_Validar(SqlConnection connection, SqlTransaction transaction, AfRenunciaProceso r)
    {
        AF_CR_Renuncias_Datos_Validar(r);
        var existe = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM socios WHERE cedula = @Cedula", r, transaction);
        if (existe == 0) throw new InvalidOperationException("La persona no existe.");
        var activa = connection.ExecuteScalar<int>(r.CodRenuncia == 0
            ? "SELECT dbo.fxAFI_Renuncia_Activa(@Cedula)"
            : "SELECT dbo.fxAFI_Renuncia_Activa_Otra(@Cedula, @CodRenuncia)", r, transaction);
        if (activa == 1) throw new InvalidOperationException("La persona tiene otra renuncia en trámite o la actual fue liquidada.");
        var estado = connection.ExecuteScalar<string>("SELECT estado FROM AFI_CR_RENUNCIAS WHERE cod_renuncia = @CodRenuncia", r, transaction);
        if (estado == "P") throw new InvalidOperationException("Esta renuncia se encuentra perdida y no puede modificarse.");
        var patronal = connection.ExecuteScalar<string>("SELECT VALOR FROM SIF_PARAMETROS WHERE COD_PARAMETRO = 'PAT_R'", transaction: transaction);
        AF_CR_Renuncias_AportePatronal_Validar(r, patronal);
        r.AporteObrero = true;
        r.Capitalizacion = true;
        r.AhorroExtraordinario = true;
        r.AceptaPatronal = false;
        if (r.Tipo == "A") r.AcFecha = null;
    }

    private static void AF_CR_Renuncias_Datos_Validar(AfRenunciaProceso r)
    {
        if (string.IsNullOrWhiteSpace(r.Cedula) || r.IdPromotor.GetValueOrDefault() <= 0 || r.IdCausa.GetValueOrDefault() <= 0)
            throw new InvalidOperationException("Datos erróneos: verifique persona, ejecutivo y causa.");
        if (r.Tipo is not ("A" or "P")) throw new InvalidOperationException("El proceso no aplica.");
        if (r.Documento == "TE" && string.IsNullOrWhiteSpace(r.Cuenta))
            throw new InvalidOperationException("Debe indicar una cuenta bancaria.");
        if (r.Tipo == "P" && string.IsNullOrWhiteSpace(r.Boleta))
            throw new InvalidOperationException("Especifique el número de boleta de acción de personal.");
    }

    private static void AF_CR_Renuncias_AportePatronal_Validar(AfRenunciaProceso r, string? patronal)
    {
        if (r.AportePatronal == true && (r.Tipo != "P" || patronal != "S"))
            throw new InvalidOperationException("La aplicación de aporte patronal no está autorizada.");
    }

    private static decimal AF_CR_Renuncias_Monto(decimal value) => Math.Round(value, 2, MidpointRounding.ToEven);

    private static void AF_CR_Renuncias_Importes_Validar(SqlConnection connection, SqlTransaction transaction, AfRenunciaProceso r)
    {
        var p = connection.QuerySingle<AfRenunciaLiqConsultaPatrimonio>(SpLiqConsultaPatrimonio,
            new { r.Cedula }, transaction, commandType: CommandType.StoredProcedure);
        // Los checks indican aplicación a deudas; la liquidación se determina por tipo.
        var baseMonto = AF_CR_Renuncias_Monto(p.Ahorro) + AF_CR_Renuncias_Monto(p.Capitaliza) + AF_CR_Renuncias_Monto(p.Extra);
        var patronal = AF_CR_Renuncias_Monto(p.Aporte) + AF_CR_Renuncias_Monto(p.Custodia);
        var retencion = AF_CR_Renuncias_Monto(p.Renta) + AF_CR_Renuncias_Monto(p.Exc_Renta);
        var planes = AF_CR_Renuncias_Planes_Validar(connection, transaction, r);
        var liquidar = baseMonto + planes - retencion;
        if (r.Tipo == "P") liquidar += patronal + (p.Exc_Aplica == 1 ? AF_CR_Renuncias_Monto(p.Excedente) : 0);
        var disponibleDeudas = baseMonto + AF_CR_Renuncias_Monto(p.Excedente) - retencion + planes;
        if (r.AportePatronal == true) disponibleDeudas += patronal;
        // El ISR de planes se consulta con los rendimientos gravables de la lista original.
        var lista = connection.Query<AfRenunciaLiquidaListaPlanes>(SpLiquidaListaPlanes,
            new { r.Cedula, TipoLiq = r.Tipo }, transaction, commandType: CommandType.StoredProcedure).ToList();
        var rendimiento = lista.Where(x => x.Renta_Global == 1 && r.Planes.Any(y => y.CodContrato == x.Cod_Contrato && y.Marcada))
            .Sum(x => AF_CR_Renuncias_Monto(x.Rendimiento) + AF_CR_Renuncias_Monto(x.RendPendiente));
        var renta = connection.QuerySingle<AfRenunciaRentaGlobal>(SpRentaGlobal,
            new { r.Cedula, Corte = DateTime.Now, MntRetiro = rendimiento, Plan = "" }, transaction, commandType: CommandType.StoredProcedure);
        disponibleDeudas -= AF_CR_Renuncias_Monto(renta.ISR_Monto);
        AF_CR_Renuncias_Abonos_Validar(connection, transaction, r, disponibleDeudas);
        var girar = liquidar - r.Abonos.Sum(x => x.Abono);
        if (Math.Abs(r.Disponible - liquidar) > 0.01m || Math.Abs(r.TotalNeto - girar) > 0.01m || Math.Abs(r.RetenerMonto - retencion) > 0.01m)
            throw new InvalidOperationException("Los importes cambiaron. Vuelva a cargar Patrimonio y revise la liquidación antes de guardar.");
        r.Disponible = liquidar;
        r.TotalNeto = girar;
        r.RetenerMonto = retencion;
    }

    private static decimal AF_CR_Renuncias_Planes_Validar(SqlConnection connection, SqlTransaction transaction, AfRenunciaProceso r)
    {
        var actuales = connection.Query<AfRenunciaLiquidaListaPlanes>(SpLiquidaListaPlanes,
            new { r.Cedula, TipoLiq = r.Tipo }, transaction, commandType: CommandType.StoredProcedure).ToList();
        if (r.Planes.Count != actuales.Count || r.Planes.Select(x => x.CodContrato).Distinct().Count() != actuales.Count)
            throw new InvalidOperationException("La lista de planes cambió; vuelva a cargarla.");
        foreach (var plan in r.Planes)
        {
            var actual = actuales.SingleOrDefault(x => x.Cod_Contrato == plan.CodContrato)
                ?? throw new InvalidOperationException("El plan no pertenece a la liquidación.");
            var monto = AF_CR_Renuncias_Monto(actual.Aportes + actual.Rendimiento + actual.RendPendiente - actual.Multa);
            if (Math.Abs(plan.Disponible - monto) > 0.01m)
                throw new InvalidOperationException("El monto del plan cambió; vuelva a cargarlo.");
            if (r.Reingreso != true && !plan.Marcada)
                throw new InvalidOperationException("Sin reingreso deben liquidarse todos los planes.");
            plan.Disponible = monto;
            plan.CodOperadora = actual.Cod_Operadora;
            plan.CodPlan = actual.Cod_Plan;
            plan.Aportes = AF_CR_Renuncias_Monto(actual.Aportes);
            plan.Rendimientos = AF_CR_Renuncias_Monto(actual.Rendimiento);
            plan.RendPendiente = AF_CR_Renuncias_Monto(actual.RendPendiente);
            plan.Multa = AF_CR_Renuncias_Monto(actual.Multa);
            plan.CodDivisa = actual.Cod_Divisa;
            plan.TipoCambio = actual.Tipo_Cambio;
        }
        return r.Planes.Where(x => x.Marcada).Sum(x => x.Disponible);
    }

    private static void AF_CR_Renuncias_Abonos_Validar(SqlConnection connection, SqlTransaction transaction, AfRenunciaProceso r, decimal disponible)
    {
        var actuales = connection.Query<AfRenunciaLiquidacionCreditosPersona>(SpLiquidacionCreditosPersona,
            new { r.Cedula, Abono = disponible }, transaction, commandType: CommandType.StoredProcedure).ToList();
        if (r.Abonos.Count != actuales.Count || r.Abonos.Select(x => x.IdSolicitud).Distinct().Count() != actuales.Count)
            throw new InvalidOperationException("La lista de créditos cambió; vuelva a cargarla.");
        if (r.Abonos.Sum(x => x.Abono) > Math.Max(0, disponible))
            throw new InvalidOperationException("Los abonos superan el disponible de aplicación.");
        var ajuste = connection.ExecuteScalar<int>("SELECT AJUSTE_TASAS FROM causas_renuncias WHERE id_causa = @IdCausa", r, transaction);
        foreach (var abono in r.Abonos)
        {
            var actual = actuales.SingleOrDefault(x => x.Id_Solicitud == abono.IdSolicitud)
                ?? throw new InvalidOperationException("El crédito no pertenece a la persona.");
            var deuda = actual.Saldo + actual.INTC + actual.INTM + actual.Cargos + actual.Polizas;
            if (ajuste != 1 && abono.Abono != AF_CR_Renuncias_Monto(actual.Abono))
                throw new InvalidOperationException("La causa no permite modificar abonos manualmente; aplique distribución automática.");
            if (abono.Abono < 0 || abono.Abono > AF_CR_Renuncias_Monto(deuda))
                throw new InvalidOperationException("El abono debe estar entre cero y el total adeudado.");
            abono.Codigo = actual.Codigo;
            abono.Saldo = actual.Saldo;
            abono.Cargos = actual.Cargos + actual.Polizas;
            abono.MoraIntC = actual.INTC;
            abono.MoraIntM = actual.INTM;
            abono.MoraPrin = actual.Amortiza;
            abono.Tipo = actual.Detalle;
            abono.Garantia = actual.GarantiaX;
            abono.CodDivisa = actual.Cod_Divisa;
            abono.TipoCambio = actual.Tipo_Cambio;
        }
    }
}


