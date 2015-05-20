﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Feature.cs">
//   Copyright (c) by respective owners including Yahoo!, Microsoft, and
//   individual contributors. All rights reserved.  Released under a BSD
//   license as described in the file LICENSE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using Microsoft.Research.MachineLearning.Serializer.Interfaces;

namespace Microsoft.Research.MachineLearning.Serializer.Intermediate
{
    public class Feature : IFeature
    {
        /// <summary>
        /// The targeted namespace.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// The targeted feature group.
        /// </summary>
        public char? FeatureGroup { get; set; }

        /// <summary>
        /// The origin property name is used as the feature name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// If true, features will be converted to string and then hashed.
        /// In VW line format: Age:15 (Enumerize=false), Age_15 (Enumerize=true)
        /// Defaults to false.
        /// </summary>
        public bool Enumerize { get; set;  }
    }

    /// <summary>
    /// The typed representation of the feature.
    /// </summary>
    /// <typeparam name="T">Type of feature value.</typeparam>
    /// <typeparam name="TResult">Result type produved by visitor.</typeparam>
    public sealed class Feature<T, TResult> : Feature, IFeature<T>, IVisitableFeature<TResult>
    {
        /// <summary>
        /// The actual value
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Compiled func to enable automatic double dispatch.
        /// </summary>
        public Func<TResult> Visit { get; set;  }
    }
}
