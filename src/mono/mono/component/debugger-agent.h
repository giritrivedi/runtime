// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//

#ifndef __MONO_DEBUGGER_AGENT_COMPONENT_H__
#define __MONO_DEBUGGER_AGENT_COMPONENT_H__

#include <mono/mini/mini.h>
#include "debugger.h"
#include <mono/utils/mono-stack-unwinding.h>

// The lowest common supported semaphore length, including null character
// NetBSD-7.99.25: 15 characters
// MacOSX 10.11: 31 -- Core 1.0 RC2 compatibility
#if defined(__NetBSD__)
#define CLR_SEM_MAX_NAMELEN 15
#elif defined(__APPLE__)
#define CLR_SEM_MAX_NAMELEN PSEMNAMLEN
#elif defined(NAME_MAX)
#define CLR_SEM_MAX_NAMELEN (NAME_MAX - 4)
#else
// On Solaris, MAXNAMLEN is 512, which is higher than MAX_PATH defined by pal.h
#define CLR_SEM_MAX_NAMELEN MAX_PATH
#endif
#define ASSERT(...)

/* define NOTRACE as nothing; this will absorb the variable-argument list used
 *    in tracing macros */
#define NOTRACE(...)
#define WARN      NOTRACE

#define HashSemaphoreName(a,b) a,b

// Build the semaphore names using the PID and a value that can be used for distinguishing
// between processes with the same PID (which ran at different times). This is to avoid
// cases where a prior process with the same PID exited abnormally without having a chance
// to clean up its semaphore.
// Note to anyone modifying these names in the future: Semaphore names on OS X are limited
// to SEM_NAME_LEN characters, including null. SEM_NAME_LEN is 31 (at least on OS X 10.11).
// NetBSD limits semaphore names to 15 characters, including null (at least up to 7.99.25).
// Keep 31 length for Core 1.0 RC2 compatibility
#if defined(__NetBSD__)
static const char* RuntimeSemaphoreNameFormat = "/clr%s%08llx";
#else
static const char* RuntimeSemaphoreNameFormat = "/clr%s%08x%016llx";
#endif

#define ERROR_INVALID_HANDLE 6L
#if !defined (_SECURECRT_FILL_BUFFER_THRESHOLD)
#ifdef _DEBUG
#define _SECURECRT_FILL_BUFFER_THRESHOLD ((size_t)8)
#else  /* _DEBUG */
#define _SECURECRT_FILL_BUFFER_THRESHOLD ((size_t)0)
#endif  /* _DEBUG */
#endif  /* !defined (_SECURECRT_FILL_BUFFER_THRESHOLD) */

/* _SECURECRT_FILL_BUFFER_PATTERN is the same as _bNoMansLandFill */
#define _SECURECRT_FILL_BUFFER_PATTERN 0xFD

/* Assert message and Invalid parameter */
#ifdef _DEBUG
    #define _ASSERT_EXPR( val, exp )                                            \
        {                                                                       \
            if ( ( val ) == 0 )                                                 \
            {                                                                   \
                if ( sMBUSafeCRTAssertFunc != NULL )                            \
                {                                                               \
                    ( *sMBUSafeCRTAssertFunc )( #exp, "SafeCRT assert failed", __FILE__, __LINE__ );    \
                }                                                               \
            }                                                                   \
        }
    #define _INVALID_PARAMETER( exp )   _ASSERT_EXPR( 0, exp )
    #define _ASSERTE( exp ) _ASSERT_EXPR( exp, exp )
#else
    #define _ASSERT_EXPR( val, expr )
    #define _INVALID_PARAMETER( exp )
    #define _ASSERTE( exp )
#endif

#define _VALIDATE_RETURN( expr, errorcode, retexpr )                           \
    {                                                                          \
        int _Expr_val=!!(expr);                                                \
        _ASSERT_EXPR( ( _Expr_val ), #expr );                       \
        if ( !( _Expr_val ) )                                                  \
        {                                                                      \
            errno = errorcode;                                                 \
            _INVALID_PARAMETER(#expr );                             \
            return ( retexpr );                                                \
        }                                                                      \
    }
/* _VALIDATE_RETURN */

#define _SECURECRT__FILL_STRING(_String, _Size, _Offset)                            \
    if ((_Size) != ((size_t)-1) && (_Size) != INT_MAX &&                            \
        ((size_t)(_Offset)) < (_Size))                                              \
    {                                                                               \
        memset((_String) + (_Offset),                                               \
            _SECURECRT_FILL_BUFFER_PATTERN,                                         \
            (_SECURECRT_FILL_BUFFER_THRESHOLD < ((size_t)((_Size) - (_Offset))) ?   \
                _SECURECRT_FILL_BUFFER_THRESHOLD :                                  \
                ((_Size) - (_Offset))) * sizeof(*(_String)));                       \
    }

/* define NOTRACE as nothing; this will absorb the variable-argument list used
 *    in tracing macros */
#define NOTRACE(...)
#define TRACE     NOTRACE
static int __cdecl vsprintf_s (
        char *string,
        size_t sizeInBytes,
        const char *format,
        va_list ap
        )
{
    int retvalue = -1;

    /* validation section */
    _VALIDATE_RETURN(format != NULL, EINVAL, -1);
    _VALIDATE_RETURN(string != NULL && sizeInBytes > 0, EINVAL, -1);

    retvalue = vsnprintf(string, sizeInBytes, format, ap);
    if (retvalue < 0)
    {
        string[0] = '\0';
        _SECURECRT__FILL_STRING(string, sizeInBytes, 1);
    }
    if (retvalue > (int)sizeInBytes)
    {
        _VALIDATE_RETURN(("Buffer too small" && 0), ERANGE, -1);
    }
    if (retvalue >= 0)
    {
        _SECURECRT__FILL_STRING(string, sizeInBytes, retvalue + 1);
    }

    return retvalue;
}

static int sprintf_s (
        char *string,
        size_t sizeInBytes,
        const char *format,
        ...
        )
{
        int ret;
        va_list arglist;
        va_start(arglist, format);
        ret = vsprintf_s(string, sizeInBytes, format, arglist);
        va_end(arglist);
        return ret;
}


void
debugger_agent_add_function_pointers (MonoComponentDebugger* fn_table);

void
mono_ss_calculate_framecount (void *the_tls, MonoContext *ctx, gboolean force_use_ctx, DbgEngineStackFrame ***frames, int *nframes);

void
mono_ss_discard_frame_context (void *the_tls);

#ifdef TARGET_WASM
DebuggerTlsData*
mono_wasm_get_tls (void);

void
mono_init_debugger_agent_for_wasm (int log_level, MonoProfilerHandle *prof);

void 
mono_change_log_level (int new_log_level);

void
mono_wasm_save_thread_context (void);

bool
mono_wasm_is_breakpoint_and_stepping_disabled (void);
#endif

void
mini_wasm_debugger_add_function_pointers (MonoComponentDebugger* fn_table);

void
mini_wasi_debugger_add_function_pointers (MonoComponentDebugger* fn_table);

#if defined(HOST_WASI)
void
mono_wasi_suspend_current (void);

void
mono_debugger_agent_initialize_function_pointers (void *start_debugger_thread, void *suspend_vm, void *suspend_current);
#endif

MdbgProtErrorCode
mono_do_invoke_method (DebuggerTlsData *tls, MdbgProtBuffer *buf, InvokeData *invoke, guint8 *p, guint8 **endp);

MdbgProtErrorCode
mono_process_dbg_packet (int id, MdbgProtCommandSet command_set, int command, gboolean *no_reply, guint8 *p, guint8 *end, MdbgProtBuffer *buf);

void
mono_dbg_process_breakpoint_events (void *_evts, MonoMethod *method, MonoContext *ctx, int il_offset);

void*
mono_dbg_create_breakpoint_events (GPtrArray *ss_reqs, GPtrArray *bp_reqs, MonoJitInfo *ji, MdbgProtEventKind kind);

int
mono_ss_create_init_args (SingleStepReq *ss_req, SingleStepArgs *args);

void
mono_ss_args_destroy (SingleStepArgs *ss_args);

int
mono_de_frame_async_id (DbgEngineStackFrame *frame);

bool
mono_debugger_agent_receive_and_process_command (void);

bool
mono_begin_breakpoint_processing (void *the_tls, MonoContext *ctx, MonoJitInfo *ji, gboolean from_signal);

void 
mono_dbg_assembly_load (MonoProfiler *prof, MonoAssembly *assembly);
#endif
