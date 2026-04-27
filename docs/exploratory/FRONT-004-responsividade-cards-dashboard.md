# FRONT-004 - Cards principais do dashboard perdem legibilidade em resolução reduzida

## Tipo
Bug visual / Responsividade / Interface

## Descrição
Durante a análise manual da interface, foi identificado que os cards principais do dashboard apresentam problema de responsividade em resoluções reduzidas.

Ao diminuir a largura da janela, os cards de `Saldo Atual`, `Receitas do Mês` e `Despesas do Mês` ficam comprimidos, com textos cortados ou parcialmente ocultos.

## Comportamento esperado
Os cards do dashboard deveriam se adaptar corretamente ao tamanho da tela, mantendo textos, valores e ícones visíveis e legíveis.

Em telas menores, os cards poderiam ser reorganizados em coluna, quebrar linha ou ajustar proporcionalmente o conteúdo para preservar a usabilidade.

## Comportamento obtido
Em resolução reduzida, os cards permanecem lado a lado e ficam comprimidos, prejudicando a leitura dos textos e valores.

Alguns elementos importantes deixam de aparecer corretamente ou ficam parcialmente cortados.

## Passos para reproduzir

1. Acessar a aplicação pelo frontend.
2. Navegar até a tela inicial/dashboard.
3. Reduzir a largura da janela do navegador ou utilizar o modo responsivo do DevTools.
4. Observar os cards principais do dashboard.
5. Verificar que textos e valores dos cards ficam cortados, comprimidos ou com baixa legibilidade.

## Impacto
O problema prejudica a leitura dos principais indicadores financeiros do dashboard em telas menores.

Embora não bloqueie a navegação, afeta a usabilidade, a acessibilidade visual e a experiência do usuário.

## Severidade
Média

## Justificativa da severidade
A falha impacta informações centrais do dashboard, como saldo, receitas e despesas. Como esses dados são parte importante da experiência inicial do usuário, a perda de legibilidade em telas menores representa um problema relevante de responsividade.

## Evidência

![Problema de responsividade nos cards do dashboard](../images/FRONT-004-responsividade-cards-dashboard.png)