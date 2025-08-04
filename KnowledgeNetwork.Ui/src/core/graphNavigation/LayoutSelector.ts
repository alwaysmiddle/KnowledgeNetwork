import { LayoutRule, GraphContext } from './types';

export class LayoutSelector {
  private rules: LayoutRule[] = [];
  
  addRule(rule: LayoutRule): void {
    this.rules.push(rule);
  }
  
  removeRule(condition: (rule: LayoutRule) => boolean): void {
    this.rules = this.rules.filter(rule => !condition(rule));
  }
  
  selectLayout(context: GraphContext): string {
    for (const rule of this.rules) {
      if (rule.condition(context)) {
        return rule.layout;
      }
    }
    return 'hierarchical'; // default
  }
  
  // Predefined rule factory methods
  static createDenseGraphRule(threshold: number = 1.5): LayoutRule {
    return {
      condition: (ctx) => ctx.edgeCount > ctx.nodeCount * threshold,
      layout: 'force-directed'
    };
  }
  
  static createOrganizationalRule(): LayoutRule {
    return {
      condition: (ctx) => ctx.metadata?.type === 'organizational',
      layout: 'hierarchical'
    };
  }
  
  static createNetworkRule(): LayoutRule {
    return {
      condition: (ctx) => ctx.metadata?.type === 'network' && ctx.hasCenter,
      layout: 'radial'
    };
  }
  
  static createLargeGraphRule(nodeThreshold: number = 50): LayoutRule {
    return {
      condition: (ctx) => ctx.nodeCount > nodeThreshold,
      layout: 'force-directed'
    };
  }
}