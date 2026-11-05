// Le o valor que esta de fato em cada campo.
// O preenchimento automatico do navegador (senha salva) coloca o valor na tela sem disparar o
// evento "input": o Blazor so recebe depois da primeira interacao, e o primeiro clique em
// "Entrar" chegava com o formulario vazio. No envio, o .NET le os valores daqui antes de validar.
// O Edge desenha o proprio botao de revelar senha dentro do campo, alem do nosso (olho).
// O <input> fica no shadow DOM do fluent-text-input, onde o CSS da pagina nao chega:
// o estilo e aplicado dentro do shadow root.
export function ocultarRevelarNativo(id) {
    const raiz = document.getElementById(id)?.shadowRoot;
    if (!raiz || raiz.querySelector('style[data-educore-reveal]')) {
        return;
    }

    const estilo = document.createElement('style');
    estilo.setAttribute('data-educore-reveal', '');
    estilo.textContent = 'input::-ms-reveal, input::-ms-clear { display: none; }';
    raiz.appendChild(estilo);
}

export function lerValores(ids) {
    return ids.map(id => {
        const campo = document.getElementById(id);
        if (!campo) {
            return null;
        }

        // fluent-text-input guarda o <input> nativo no shadow DOM
        const nativo = campo.shadowRoot ? campo.shadowRoot.querySelector('input') : null;
        return (nativo ? nativo.value : campo.value) ?? null;
    });
}
