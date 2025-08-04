import { ReactFlowInstance, Node } from 'reactflow';
import { AnimateOptions } from './types';

export const animate = ({ duration, onUpdate }: AnimateOptions): Promise<void> => {
  return new Promise<void>((resolve) => {
    const start = Date.now();
    
    const frame = () => {
      const elapsed = Date.now() - start;
      const progress = Math.min(elapsed / duration, 1);
      
      // Easing function (ease-out cubic)
      const easedProgress = 1 - Math.pow(1 - progress, 3);
      
      onUpdate(easedProgress);
      
      if (progress < 1) {
        requestAnimationFrame(frame);
      } else {
        resolve();
      }
    };
    
    requestAnimationFrame(frame);
  });
};

export const animateZoomIn = async (
  targetNode: Node,
  rf: ReactFlowInstance,
  duration: number = 800
): Promise<void> => {
  const startViewport = rf.getViewport();
  const targetZoom = 2;
  
  const targetX = -targetNode.position.x * targetZoom + window.innerWidth / 2;
  const targetY = -targetNode.position.y * targetZoom + window.innerHeight / 2;
  
  return animate({
    duration,
    onUpdate: (progress) => {
      const zoom = startViewport.zoom + (targetZoom - startViewport.zoom) * progress;
      const x = startViewport.x + (targetX - startViewport.x) * progress;
      const y = startViewport.y + (targetY - startViewport.y) * progress;
      
      rf.setViewport({ x, y, zoom });
    }
  });
};

export const animateZoomOut = async (
  rf: ReactFlowInstance,
  duration: number = 800
): Promise<void> => {
  const startViewport = rf.getViewport();
  const targetZoom = 0.5;
  
  return animate({
    duration,
    onUpdate: (progress) => {
      const zoom = startViewport.zoom + (targetZoom - startViewport.zoom) * progress;
      rf.setViewport({ ...startViewport, zoom });
    }
  });
};

export const fadeTransition = (duration: number = 300): Promise<void> => {
  return new Promise((resolve) => {
    setTimeout(resolve, duration);
  });
};

export const sleep = (ms: number): Promise<void> => {
  return new Promise((resolve) => setTimeout(resolve, ms));
};