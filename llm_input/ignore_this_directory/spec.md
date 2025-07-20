# MetaTypes Library Specification

## Overview

MetaTypes is a .NET library that provides compile-time type metadata generation through source generators. It analyzes classes marked with `[MetaType]` and generates corresponding MetaType classes that offer both static compile-time access and runtime polymorphic capabilities through a singleton instance pattern.

## Core Concepts

The library enables:
- **Compile-time metadata access** without runtime reflection overhead
- **Static type-safe APIs** for known types at build time  
- **Runtime polymorphic access** for dynamic scenarios
- **Cross-type references** between MetaTypes
- **Zero-overhead abstraction** over reflection

## Architecture

# Interfaces and Contracts
See [interfaces.md](interfaces.md) for complete interface definitions and contracts.

# Generated Code Structure  
See [generated-code.md](generated-code.md) for templates and examples of generated MetaType classes.

# Usage Patterns and Examples
See [usage-examples.md](usage-examples.md) for comprehensive usage scenarios and code patterns.

# Implementation Roadmap
See [implementation-roadmap.md](implementation-roadmap.md) for phase-by-phase implementation plan.

# Project Structure and Configuration
See [project-structure.md](project-structure.md) for solution organization, build configuration, and packaging.

# Source Generator Implementation
See [source-generator-guide.md](source-generator-guide.md) for detailed generator implementation requirements.

# Testing Strategy
See [testing-strategy.md](testing-strategy.md) for unit testing, integration testing, and validation approaches.

# Performance and Best Practices
See [performance-guide.md](performance-guide.md) for optimization strategies and coding standards.

# Deployment and Distribution
See [deployment-guide.md](deployment-guide.md) for NuGet packaging and distribution requirements.