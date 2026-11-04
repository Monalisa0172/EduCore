// Protege o formulario contra cliques em links internos enquanto ha alteracoes nao salvas.
// Com interatividade por pagina, o blazor.web.js navega os links (navegacao aprimorada) sem passar
// pelo NavigationLock do WebAssembly. Aqui o clique e capturado antes e devolvido ao .NET,
// que navega pelo NavigationManager - e ai o NavigationLock abre o dialogo de confirmacao.

let protecao = null;

function linkInterno(evento) {
    // composedPath: inclui elementos dentro de web components (ex.: itens do menu do Fluent)
    const alvo = evento.composedPath().find(el =>
        el instanceof Element &&
        (el.tagName === 'A' || el.tagName.startsWith('FLUENT-')) &&
        el.getAttribute('href') !== null);

    if (!alvo || alvo.hasAttribute('download')) {
        return null;
    }

    const destinoAba = alvo.getAttribute('target');
    if (destinoAba && destinoAba !== '_self') {
        return null;
    }

    const url = new URL(alvo.getAttribute('href'), document.baseURI);
    return url.href.startsWith(document.baseURI) ? url.href : null;
}

export function ativar(dotNetRef) {
    desativar();

    protecao = evento => {
        // Ctrl/Shift/clique do meio abrem nova aba: nao ha o que proteger
        if (evento.defaultPrevented || evento.button !== 0 ||
            evento.ctrlKey || evento.metaKey || evento.shiftKey || evento.altKey) {
            return;
        }

        const url = linkInterno(evento);
        if (!url) {
            return;
        }

        evento.preventDefault();
        evento.stopImmediatePropagation();
        dotNetRef.invokeMethodAsync('NavegarProtegido', url);
    };

    // Fase de captura na window: roda antes do listener do blazor.web.js e do JS dos componentes
    window.addEventListener('click', protecao, true);
}

export function desativar() {
    if (protecao) {
        window.removeEventListener('click', protecao, true);
        protecao = null;
    }
}
